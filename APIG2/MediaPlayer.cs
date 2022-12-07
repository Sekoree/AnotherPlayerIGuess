using System;
using System.Threading.Tasks;
using APIG2.Interfaces;
using APIG2.Messages;
using CommunityToolkit.Mvvm.Messaging;
using ManagedBass;

namespace APIG2;

public class MediaPlayer
{
    private readonly string[] _plugins = { "bass_aac", "basshls", "bassopus", "basswebm" };

    private int _streamHandle = -1;
    private IBaseTrack? _currentTrack;

    public PlaybackState State => _streamHandle == -1 ? PlaybackState.Stopped : Bass.ChannelIsActive(_streamHandle);

    private float _volumeInternal = 1f;

    public float Volume
    {
        get => _volumeInternal;
        set
        {
            _volumeInternal = value;
            if (_streamHandle != -1) 
                Bass.ChannelSetAttribute(_streamHandle, ChannelAttribute.Volume, _volumeInternal);
        }
    }

    private float[] _lastFftData = new float[2048];

    public bool Init()
    {
        var bass = Bass.Init();
        if (!bass)
        {
            App.Messenger
                .Send(new MediaPlayerError("Bass.Init() failed", Bass.LastError));
            return false;
        }

        foreach (var plugin in _plugins)
        {
            var pluginLoaded = Bass.PluginLoad(plugin);
            if (pluginLoaded != 0)
                continue;
            App.Messenger.Send(new MediaPlayerError($"Bass.PluginLoad(\"{plugin}\") failed", Bass.LastError));
        }

        Bass.UpdatePeriod = 1000 / 60;

        return true;
    }


    private readonly object _lock = new();

    public async Task<bool> LoadTrackAsync(IBaseTrack track)
    {
        if (track.PrepStatus == PreparationStatus.NotPrepared)
            _ = Task.Run(track.PrepareTrackAsync);

        while (track.PrepStatus is not PreparationStatus.Prepared or PreparationStatus.Failed)
            await Task.Delay(100);

        if (track.PrepStatus == PreparationStatus.Failed)
            return false;

        lock (_lock)
        {
            if (_streamHandle != -1)
                Bass.StreamFree(_streamHandle);

            _streamHandle =
                Bass.CreateStream(track.AudioStream!, 0, track.AudioStream!.Length, BassFlags.Float);
            if (_streamHandle == -1)
            {
                App.Messenger.Send(new MediaPlayerError("Bass.CreateStream() failed", Bass.LastError));
                return false;
            }

            Bass.ChannelSetAttribute(_streamHandle, ChannelAttribute.Volume, Volume);
            Bass.ChannelSetAttribute(_streamHandle, ChannelAttribute.Granule, 512);
            Bass.ChannelSetDSP(_streamHandle, FftProc, IntPtr.Zero, 1);
            Bass.ChannelSetDSP(_streamHandle, PositionProc, IntPtr.Zero, 0);
            Bass.ChannelSetSync(_streamHandle, SyncFlags.End, 0, EndProc, IntPtr.Zero);

            _currentTrack = track;
            App.Messenger.Send(new MediaPlayerTrackLoaded(track));
        }

        return true;
    }

    private void EndProc(int handle, int channel, int data, nint user)
        => App.Messenger.Send(new MediaPlayerTrackEnded(_currentTrack!));


    private void FftProc(int handle, int channel, nint buffer, int length, nint user)
    {
        var data = new float[4096];
        var dataLength = Bass.ChannelGetData(channel, data, (int)DataFlags.FFT8192);
        if (dataLength == -1)
            return;
        data = data[..2048];

        for (var i = 0; i < data.Length; i++)
            data[i] = ((_lastFftData[i] * 0.85f) + data[i]) * 0.5f;

        _lastFftData = data;

        App.Messenger.Send(new MediaPlayerTrackFftRendered(data));
    }


    private void PositionProc(int handle, int channel, nint buffer, int length, nint user)
        => App.Messenger.Send(new MediaPlayerTrackPositionChanged(
            TimeSpan.FromSeconds(
                Bass.ChannelBytes2Seconds(channel,
                    Bass.ChannelGetPosition(channel)))));

    public void Play()
    {
        if (_streamHandle == -1)
        {
            App.Messenger.Send(new MediaPlayerError("No track loaded"));
            return;
        }

        Bass.ChannelPlay(_streamHandle);
        App.Messenger.Send(new MediaPlayerPlaybackStatusChanged(this.State));
    }

    public void Pause()
    {
        if (_streamHandle == -1)
        {
            App.Messenger.Send(new MediaPlayerError("No track loaded"));
            return;
        }

        Bass.ChannelPause(_streamHandle);
        App.Messenger.Send(new MediaPlayerPlaybackStatusChanged(this.State));
    }

    public void Resume()
    {
        if (_streamHandle == -1)
        {
            App.Messenger.Send(new MediaPlayerError("No track loaded"));
            return;
        }

        Bass.ChannelPlay(_streamHandle);
        App.Messenger.Send(new MediaPlayerPlaybackStatusChanged(this.State));
    }

    public void Stop()
    {
        if (_streamHandle == -1)
        {
            App.Messenger.Send(new MediaPlayerError("No track loaded"));
            return;
        }

        Bass.ChannelStop(_streamHandle);
        App.Messenger.Send(new MediaPlayerPlaybackStatusChanged(PlaybackState.Stopped));
    }
}