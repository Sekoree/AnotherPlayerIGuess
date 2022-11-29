using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APIG.UI.EventArgs;
using APIG.UI.Models;
using ManagedBass;

namespace APIG.UI;

public class MediaPlayer
{
    private readonly string[] pluginsToLoad = { "bass_aac", "basshls", "bassopus", "basswebm" };

    public event EventHandler<PlayerErroredEventArgs>? PlayerErrored;
    public event EventHandler<TrackLoadErroredEventArgs>? TrackLoadErrored;
    public event EventHandler<TrackLoadedEventArgs>? TrackLoaded;
    public event EventHandler<TrackStartedEventArgs>? TrackStarted;
    public event EventHandler<TrackResumedEventArgs>? TrackResumed;
    public event EventHandler<TrackStoppedEventArgs>? TrackStopped;
    public event EventHandler<TrackPausedEventArgs>? TrackPaused;
    public event EventHandler<TrackFinishedEventArgs>? TrackFinished;
    public event EventHandler<TrackPositionChangedEventArgs>? TrackPositionChanged;
    public event EventHandler<TrackFftsRenderedEventArgs>? TrackFftsRendered;

    private int _streamHandle = -1;
    private IBaseTrack? _currentTrack;
    private double _volume = 1.0f;

    private float[] _currentMaxFfts = new float[4096];
    private float[] _lastFfts = new float[4096];

    public double Volume
    {
        get
        {
            if (_streamHandle == -1)
                return _volume;
            var bassVolume = Bass.ChannelGetAttribute(_streamHandle, ChannelAttribute.Volume);
            if (_volume != bassVolume)
                Bass.ChannelSetAttribute(_streamHandle, ChannelAttribute.Volume, (float)_volume);
            return Bass.ChannelGetAttribute(_streamHandle, ChannelAttribute.Volume);
        }
        set
        {
            _volume = Math.Clamp(value, 0.0, 1.0);
            if (_streamHandle != -1)
                Bass.ChannelSetAttribute(_streamHandle, ChannelAttribute.Volume, Math.Clamp(value, 0.0, 1.0));
        }
    }

    public PlaybackState? PlaybackState => _streamHandle == -1 ? null : Bass.ChannelIsActive(_streamHandle);

    public MediaPlayer()
    {
    }

    public void Init()
    {
        var couldInit = Bass.Init();
        if (!couldInit)
            PlayerErrored?.Invoke(this, new PlayerErroredEventArgs(Bass.LastError, "Could not initialize BASS"));

        foreach (var plugin in pluginsToLoad)
        {
            var id = Bass.PluginLoad(plugin);
            if (id == 0)
                PlayerErrored?.Invoke(this,
                    new PlayerErroredEventArgs(Bass.LastError, $"Could not load plugin {plugin}"));
        }

        Bass.NetPreBuffer = 0;
        Bass.NetBufferLength = 100;
        Bass.UpdatePeriod = 1000 / 60;
    }


    public static object _trackChangeLock = new object();

    public async Task<bool> LoadTrackAsync(IBaseTrack track)
    {
        var path = await track.GetPathAsync();
        if (path is null)
        {
            TrackLoadErrored?.Invoke(this,
                new TrackLoadErroredEventArgs(track, new Exception("Could not get path")));
            return false;
        }

        lock (_trackChangeLock)
        {
            //Free the current stream is not already freed
            if (_streamHandle != -1)
                Bass.StreamFree(_streamHandle);

            //Path.ToString and try to create a stream from URL, with StreamDownloadBlocks and Float flags
            _streamHandle =
                Bass.CreateStream(path.ToString(), 0, BassFlags.StreamDownloadBlocks | BassFlags.Float, null);
            
            if (_streamHandle == 0)
            {
                TrackLoadErrored?.Invoke(this,
                    new TrackLoadErroredEventArgs(track,
                        new Exception("Could not create stream, Bass error: " + Bass.LastError)));
                return false;
            }
            _currentTrack = track;

            TrackLoaded?.Invoke(this, new TrackLoadedEventArgs(track));
            return true;
        }
    }

    public void Play()
    {
        //check if stream is valid
        if (_streamHandle == -1)
        {
            PlayerErrored?.Invoke(this,
                new PlayerErroredEventArgs(null, "Could not play, stream is invalid"));
            return;
        }

        //Set volume
        var couldVolume = Bass.ChannelSetAttribute(_streamHandle, ChannelAttribute.Volume, Volume);
        if (!couldVolume)
            PlayerErrored?.Invoke(this,
                new PlayerErroredEventArgs(Bass.LastError, "Could not set volume"));
        //Set granule to 512
        var couldGranule = Bass.ChannelSetAttribute(_streamHandle, ChannelAttribute.Granule, 512);
        if (!couldGranule)
            PlayerErrored?.Invoke(this,
                new PlayerErroredEventArgs(Bass.LastError, "Could not set granule"));
        //Set sync to end of stream
        var couldSync = Bass.ChannelSetSync(_streamHandle, SyncFlags.End, 0,
            (_, _, _, _) => { TrackFinished?.Invoke(this, new TrackFinishedEventArgs(_currentTrack)); });
        if (couldSync == 0)
            PlayerErrored?.Invoke(this,
                new PlayerErroredEventArgs(Bass.LastError, "Could not set sync"));
        //Set DSP to get position
        var couldDsp = Bass.ChannelSetDSP(_streamHandle, DspProcedure, IntPtr.Zero, 0);
        if (couldDsp == 0)
            PlayerErrored?.Invoke(this,
                new PlayerErroredEventArgs(Bass.LastError, "Could not set DSP"));
        //Play stream
        var couldPlay = Bass.ChannelPlay(_streamHandle);
        if (!couldPlay)
        {
            PlayerErrored?.Invoke(this,
                new PlayerErroredEventArgs(Bass.LastError, "Could not play, Bass error: " + Bass.LastError));
            return;
        }
        TrackStarted?.Invoke(this, new TrackStartedEventArgs(_currentTrack));
    }

    private void DspProcedure(int handle, int channel, nint buffer, int length, nint user)
    {
        var position = Bass.ChannelBytes2Seconds(channel, Bass.ChannelGetPosition(channel));
        TrackPositionChanged?.Invoke(this, new TrackPositionChangedEventArgs(TimeSpan.FromSeconds(position)));

        var ffts = new float[8192];
        var dataLength = Bass.ChannelGetData(channel, ffts, (int)DataFlags.FFT16384);
        if (dataLength == -1) return;

        //only use hearable frequencies/ first half of ffts
        var half = ffts.Length / 2;
        var hearableFfts = ffts[..half];

        //decrease last max ffts by 10%
        for (var i = 0; i < _currentMaxFfts.Length; i++) _currentMaxFfts[i] *= 0.9999999f;

        //set max ffts
        for (var i = 0; i < hearableFfts.Length; i++)
        {
            if (hearableFfts[i] > _currentMaxFfts[i]) _currentMaxFfts[i] = ffts[i];
        }

        //var smoothedFfts = SmoothBands(hearableFfts, 0, 0);

        //average with last ffts * 0.8
        for (var i = 0; i < hearableFfts.Length; i++)
        {
            hearableFfts[i] = ((_lastFfts[i] * 0.8f) + hearableFfts[i]) / 2;
        }

        //set last ffts
        _lastFfts = hearableFfts;

        TrackFftsRendered?.Invoke(this, new TrackFftsRenderedEventArgs(hearableFfts, _currentMaxFfts));
    }

    public void Pause()
    {
        //Check if stream is valid
        if (_streamHandle == -1)
        {
            PlayerErrored?.Invoke(this,
                new PlayerErroredEventArgs(null, "Could not pause, stream is invalid"));
            return;
        }

        //PauseAsync stream
        Bass.ChannelPause(_streamHandle);
        TrackPaused?.Invoke(this, new TrackPausedEventArgs(_currentTrack!));
    }

    public void Resume()
    {
        //Check if stream is valid
        if (_streamHandle == -1)
        {
            PlayerErrored?.Invoke(this,
                new PlayerErroredEventArgs(null, "Could not resume, stream is invalid"));
            return;
        }

        //Resume stream
        Bass.ChannelPlay(_streamHandle);
        TrackResumed?.Invoke(this, new TrackResumedEventArgs(_currentTrack!));
    }

    public void Stop()
    {
        //Check if stream is valid
        if (_streamHandle == -1)
        {
            PlayerErrored?.Invoke(this,
                new PlayerErroredEventArgs(null, "Could not stop, stream is invalid"));
            return;
        }

        //Stop stream
        Bass.ChannelStop(_streamHandle);
        //Free stream
        Bass.StreamFree(_streamHandle);
        TrackStopped?.Invoke(this, new TrackStoppedEventArgs(_currentTrack!));
    }

    public long GetPositionBytes()
    {
        //Check if stream is valid
        if (_streamHandle != -1)
            return Bass.ChannelGetPosition(_streamHandle);

        PlayerErrored?.Invoke(this,
            new PlayerErroredEventArgs(null, "Could not get position, stream is invalid"));
        return -1;
    }
}