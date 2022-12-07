using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using APIG2.Interfaces;
using APIG2.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ManagedBass;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace APIG2.Models;

[INotifyPropertyChanged]
public partial class YouTubeTrack : IBaseTrack
{
    private static readonly YoutubeClient _client = new();

    #region Base Info

    [ObservableProperty] private string _title = string.Empty;

    [ObservableProperty] private string _artist = string.Empty;

    [ObservableProperty] private TimeSpan _duration = TimeSpan.Zero;

    [ObservableProperty] private string _albumArtUri = string.Empty;

    [ObservableProperty] private Uri? _source;

    [ObservableProperty] private bool _infoLoaded = false;

    #endregion

    #region Data

    [ObservableProperty] private double _streamPercentLoaded = 0;

    [ObservableProperty] private PreparationStatus _prepStatus = PreparationStatus.NotPrepared;

    [ObservableProperty] private float[] _fftMaxPerSegment = Array.Empty<float>();

    [ObservableProperty] private double _loudness = 0;

    public byte[]? AudioStream { get; set; }

    #endregion

    #region Constructors

    public YouTubeTrack()
    {
    }

    public YouTubeTrack(string url)
    {
        Source = new Uri(url);
    }

    public YouTubeTrack(string url, string title, string artist, TimeSpan duration, string albumArtUri)
    {
        Source = new Uri(url);
        Title = title;
        Artist = artist;
        Duration = duration;
        AlbumArtUri = albumArtUri;
        InfoLoaded = true;
    }

    public YouTubeTrack(IBaseTrack track)
    {
        Source = track.Source;
        Title = track.Title;
        Artist = track.Artist;
        Duration = track.Duration;
        AlbumArtUri = track.AlbumArtUri;
        InfoLoaded = track.InfoLoaded;
    }

    #endregion

    public async Task<bool> LoadInfoAsync()
    {
        try
        {
            if (Source is null)
            {
                App.Messenger.Send(new TrackPrepareError("YouTubeTrack: Source is null"));
                return false;
            }

            var videoId = VideoId.TryParse(Source.ToString());
            if (videoId is null)
            {
                App.Messenger.Send(new TrackPrepareError("YouTubeTrack: VideoId is null or invalid" +
                                                         "\n Source: " + Source));
                return false;
            }

            var video = await _client.Videos.GetAsync(videoId.Value);
            Title = video.Title;
            Artist = video.Author.ChannelTitle;
            Duration = video.Duration ?? TimeSpan.Zero;
            AlbumArtUri = video.Thumbnails.GetWithHighestResolution().Url;
            InfoLoaded = true;
            return true;
        }
        catch (Exception e)
        {
            App.Messenger.Send(new TrackPrepareError("YouTubeTrack: Failed to load info", e));
            return false;
        }
    }

    public async Task<bool> LoadAudioStreamAsync()
    {
        try
        {
            var stream = await _client.Videos.Streams.GetManifestAsync(Source!.ToString());
            var audioStream = stream.GetAudioOnlyStreams().TryGetWithHighestBitrate();
            if (audioStream is null)
            {
                App.Messenger.Send(new TrackError("YouTubeTrack: AudioStream is null"));
                return false;
            }

            var timeStarted = DateTime.Now;
            var tempStream = new MemoryStream();
            await _client.Videos.Streams.CopyToAsync(audioStream, tempStream,
                new Progress<double>((percent) =>
                {
                    StreamPercentLoaded = percent;
                    var timeElapsed = DateTime.Now - timeStarted;
                    if (timeElapsed.TotalSeconds > 2.5)
                    {
                        App.Messenger.Send(new TrackPreparationSlow(this));
                    }
                }));
            
            AudioStream = tempStream.ToArray();
            await tempStream.DisposeAsync();
            
            StreamPercentLoaded = 1;
            App.Messenger.Send(new TrackPreparationDone(this));
            return true;
        }
        catch (Exception e)
        {
            App.Messenger.Send(new TrackError("YouTubeTrack: Failed to load audio stream", e));
            return false;
        }
    }

    public Task<bool> LoadFftDataAsync()
    {
        try
        {
            if ((int)StreamPercentLoaded != 1)
            {
                App.Messenger.Send(
                    new TrackPrepareError("YouTubeTrack: AudioStream is not fully loaded"));
                return Task.FromResult(false);
            }

            var decodeChannel = Bass.CreateStream(AudioStream!, 0, AudioStream!.Length,
                BassFlags.Decode | BassFlags.Float | BassFlags.Prescan);
            if (decodeChannel == 0)
            {
                App.Messenger.Send(new TrackPrepareError(
                    "YouTubeTrack: Unable to create decode channel", bassError: Bass.LastError));
                return Task.FromResult(false);
            }

            Bass.ChannelSetAttribute(decodeChannel, ChannelAttribute.Granule, 512);

            //get all FFT8192 data
            var fft = new float[4096];
            FftMaxPerSegment = new float[2048];
            while (Bass.ChannelGetData(decodeChannel, fft, (int)(DataFlags.FFT8192 | DataFlags.Float)) > 0)
            {
                //Only use hearable ones
                fft = fft[0..2048];
                for (var i = 0; i < fft.Length; i++)
                    FftMaxPerSegment[i] = Math.Max(FftMaxPerSegment[i], fft[i]);

                fft = new float[4096];
            }

            Bass.StreamFree(decodeChannel);

            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            App.Messenger.Send(new TrackPrepareError("YouTubeTrack: Failed to load FFT data", e));
            return Task.FromResult(false);
        }
    }

    public Task<bool> LoadLoudnessAsync()
    {
        //get max dB value
        var maxDb = FftMaxPerSegment.Max();
        //convert to linear
        var maxLinear = Math.Pow(10, maxDb / 20);
        //convert to loudness
        Loudness = 1.5849 * Math.Pow(maxLinear, 0.89) - 0.6685;
        return Task.FromResult(true);
        //No idea if correct, copilot suggested this
    }

    public async Task<bool> PrepareTrackAsync()
    {
        if (this.PrepStatus == PreparationStatus.Prepared)
            return true;

        PrepStatus = PreparationStatus.Preparing;
        var audio = await LoadAudioStreamAsync();
        if (!audio)
        {
            PrepStatus = PreparationStatus.Failed;
            return false;
        }

        if (FftMaxPerSegment != Array.Empty<float>())
        {
            PrepStatus = PreparationStatus.Prepared;
            return true;
        }

        var fft = await LoadFftDataAsync();
        if (!fft)
        {
            PrepStatus = PreparationStatus.Failed;
            return false;
        }

        //tbh this shouldn't fail but just in case
        var loudness = await LoadLoudnessAsync();
        if (!loudness)
        {
            PrepStatus = PreparationStatus.Failed;
            return false;
        }

        PrepStatus = PreparationStatus.Prepared;
        return true;
    }

    public async Task UnprepareTrackAsync()
    {
        try
        {
            AudioStream = null;
            GC.Collect();
            PrepStatus = PreparationStatus.NotPrepared;
        }
        catch (Exception e)
        {
            await File.AppendAllTextAsync("extraLog.txt", e.ToString());
        }
    }
}