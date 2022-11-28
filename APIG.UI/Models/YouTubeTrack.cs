using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace APIG.UI.Models;

public class YouTubeTrack : ReactiveObject, IBaseTrack
{
    private static readonly YoutubeClient _youtubeClient = new();
    private static readonly HttpClient _httpClient = new();
    private Video? _video;

    [Reactive] public string Title { get; set; } = "Unknown Title";
    [Reactive] public string Artist { get; set; } = "Unknown Artist";
    [Reactive] public TimeSpan Duration { get; set; } = TimeSpan.Zero;
    [Reactive] public string AlbumArtUri { get; set; } = string.Empty;
    [Reactive] public Uri Source { get; set; }
    [Reactive] public bool InfoLoaded { get; set; } = false;

    /// <summary>
    /// Design time constructor
    /// </summary>
    public YouTubeTrack()
    {
        Source = null!;
    }

    public YouTubeTrack(Uri source)
    {
        Source = source;
    }

    public YouTubeTrack(string title, string artist, TimeSpan duration, string albumArtUri, Uri source)
    {
        Title = title;
        Artist = artist;
        Duration = duration;
        AlbumArtUri = albumArtUri;
        Source = source;
        InfoLoaded = true;
    }

    //ctor to create copy of track
    public YouTubeTrack(IBaseTrack track)
    {
        Title = track.Title;
        Artist = track.Artist;
        Duration = track.Duration;
        AlbumArtUri = track.AlbumArtUri;
        Source = track.Source;
        InfoLoaded = track.InfoLoaded;
    }

    public async Task<bool> LoadInfoAsync(bool skipArt = false)
    {
        if (InfoLoaded)
            return true;
        try
        {
            var video = await _youtubeClient.Videos.GetAsync(Source.ToString());
            _video = video;
            Title = video.Title;
            Artist = video.Author.ChannelTitle;
            Duration = video.Duration.GetValueOrDefault();
            AlbumArtUri = video.Thumbnails.GetWithHighestResolution().Url;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return false;
        }

        return true;
    }

    public async Task<Uri?> GetPathAsync()
    {
        try
        {
            _video ??= await _youtubeClient.Videos.GetAsync(Source.ToString());
            var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(_video.Id);
            var streamInfo = streamManifest.GetAudioOnlyStreams().TryGetWithHighestBitrate();
            return streamInfo is null ? default : new Uri(streamInfo.Url);
        }
        catch (Exception e)
        {
            await File.WriteAllTextAsync("got error", e.Message);
            return default;
        }
    }
}