using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using APIG.UI.Models;
using Avalonia.Threading;
using ManagedBass;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace APIG.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly YoutubeClient _client = new();
    private readonly MediaPlayer _player;
    private readonly Random _random = new();

    #region Debug

    [Reactive] public string ErrorLog { get; set; } = string.Empty;

    #endregion

    #region Playback

    [Reactive] public ObservableCollection<IBaseTrack> LastPlayedTracks { get; set; } = new();

    [Reactive] public TimeSpan CurrentPosition { get; set; } = TimeSpan.Zero;
    [Reactive] public IBaseTrack? CurrentMedia { get; set; } = default;
    [Reactive] public double Volume { get; set; } = 1.0;
    [Reactive] public bool IsPlaying { get; set; } = false;
    [Reactive] public bool IsShuffle { get; set; } = false;
    [Reactive] public bool SaveToFile { get; set; } = false;

    #endregion

    #region Playlist Tab

    [Reactive] public string AddTrackOrPlaylistUri { get; set; } = string.Empty;
    [Reactive] public bool EntirePlaylist { get; set; } = false;
    [Reactive] public ObservableCollection<IBaseTrack> PlaylistTracks { get; set; } = new();
    private int _lastPlaylistIndexBeforeRequests = -1;

    #endregion

    #region Requests Tab

    [Reactive] public string AddRequestSong { get; set; } = string.Empty;
    [Reactive] public ObservableCollection<IBaseTrack> RequestTracks { get; set; } = new();

    #endregion

    #region Search Tab

    [Reactive] public string SearchQuery { get; set; } = string.Empty;
    [Reactive] public ObservableCollection<IBaseTrack> SearchTracks { get; set; } = new();

    #endregion

    #region MediaControl Commands

    [Reactive] public ReactiveCommand<Unit, Unit> PlayCommand { get; set; }
    [Reactive] public ReactiveCommand<Unit, Unit> PauseCommand { get; set; }
    [Reactive] public ReactiveCommand<Unit, Unit> StopCommand { get; set; }
    [Reactive] public ReactiveCommand<Unit, Unit> NextCommand { get; set; }
    [Reactive] public ReactiveCommand<Unit, Unit> PreviousCommand { get; set; }

    #endregion

    #region Visualizer

    [Reactive] public float[] CurrentFFTs { get; set; } = new float[4096];
    [Reactive] public float[] CurrentMaxFFT { get; set; } = new float[4096];

    #endregion

    public MainWindowViewModel()
    {
        _player = new MediaPlayer(this);
        _player.TrackLoaded += (_, args) =>
        {
            Dispatcher.UIThread.Post(() => CurrentMedia = args.Track);
            //Save Title and Artist to text files
            if (!SaveToFile) 
                return;
            System.IO.File.WriteAllText("Title.txt", args.Track.Title);
            System.IO.File.WriteAllText("Artist.txt", args.Track.Artist);
        };
        _player.TrackStarted += (_, _) => Dispatcher.UIThread.Post(() => IsPlaying = true);
        _player.TrackPaused += (_, _) => Dispatcher.UIThread.Post(() => IsPlaying = false);
        _player.TrackResumed += (_, _) => Dispatcher.UIThread.Post(() => IsPlaying = true);
        _player.TrackStopped += (_, _) => Dispatcher.UIThread.Post(() =>
        {
            CurrentMedia = null;
            CurrentPosition = TimeSpan.Zero;
            IsPlaying = false;
        });
        _player.TrackPositionChanged +=
            (_, args) =>
            {
                if (args.Position.Seconds != CurrentPosition.Seconds)
                    Dispatcher.UIThread.Post(() => CurrentPosition = args.Position);
            };
        _player.TrackFinished += (_, _) => Dispatcher.UIThread.Post(() => NextCommand!.Execute());
        _player.TrackLoadErrored += (_, args) => Dispatcher.UIThread.Post(() =>
        {
            Debug.WriteLine(args.Track?.Title);
            Debug.WriteLine(args.Track?.Source);
            Debug.WriteLine(args.Exception);
            NextCommand!.Execute();
        });
        _player.PlayerErrored += (_, args) => { Debug.WriteLine(args); };

        _player.TrackFftsRendered += (_, args) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                CurrentFFTs = args.Ffts;
                CurrentMaxFFT = args.MaxFft;
            });
        };

        _player.Init();

        //when any value on volume changes, update the player volume
        this.WhenAnyValue(x => x.Volume).Subscribe(x => _player.Volume = Math.Pow(x, 3));

        var canUseControls = this.WhenAnyValue(x => x.PlaylistTracks.Count).Select(x => x > 0);
        var canUseMediaSwap = canUseControls.CombineLatest(this.WhenAnyValue(x => x.IsPlaying),
            (controls, playing) => controls && playing);

        PlayCommand = ReactiveCommand.CreateFromTask(PlayAsync, canUseControls);
        PauseCommand = ReactiveCommand.Create(Pause, canUseControls);
        StopCommand = ReactiveCommand.Create(Stop, canUseControls);
        NextCommand = ReactiveCommand.CreateFromTask(NextAsync, canUseMediaSwap);
        PreviousCommand = ReactiveCommand.CreateFromTask(PreviousAsync, canUseMediaSwap);
    }

    public async Task PlayAsync()
    {
        if (_player.PlaybackState is PlaybackState.Paused)
        {
            _player.Resume();
            return;
        }

        var couldLoad = await _player.LoadTrackAsync(PlaylistTracks[0]);
        if (couldLoad)
            _player.Play();
    }

    public async Task PlaySpecificTrackAsync(IBaseTrack track)
    {
        //stop the current track
        if (CurrentMedia != null && _player.PlaybackState is PlaybackState.Playing or PlaybackState.Paused)
            _player.Stop();

        var couldLoad = await _player.LoadTrackAsync(track);
        if (couldLoad)
            _player.Play();
    }

    public void Pause()
    {
        _player.Pause();
    }

    public void Stop()
    {
        _player.Stop();
    }

    public async Task NextAsync()
    {
        var currentMedia = CurrentMedia;
        //If not coming from a Stop, add the current media to the last played list
        if (currentMedia is not null && PlaylistTracks.Contains(currentMedia))
            LastPlayedTracks.Insert(0, currentMedia);

        //If we're playing requests, remove after it was played
        if (RequestTracks.Contains(currentMedia!))
            RequestTracks.Remove(currentMedia!);

        //Change to requests while the list for it is not empty
        if (PlaylistTracks.Contains(currentMedia!) && RequestTracks.Count > 0)
        {
            _lastPlaylistIndexBeforeRequests = PlaylistTracks.IndexOf(currentMedia!);
            await _player.LoadTrackAsync(RequestTracks[0]);
            _player.Play();
        }
        //If we're playing requests and the list is not empty, play the next request
        else if (RequestTracks.Count > 0)
        {
            await _player.LoadTrackAsync(RequestTracks[0]);
            _player.Play();
        }
        //If we're playing requests and the list is empty, go back to the playlist
        else if (RequestTracks.Count == 0 && _lastPlaylistIndexBeforeRequests != -1)
        {
            var nextIndex = _lastPlaylistIndexBeforeRequests + 1;
            if (nextIndex >= PlaylistTracks.Count)
                nextIndex = 0;
            _lastPlaylistIndexBeforeRequests = -1;
            if (IsShuffle)
                nextIndex = _random.Next(0, PlaylistTracks.Count);

            await _player.LoadTrackAsync(PlaylistTracks[nextIndex]);
            _player.Play();
        }
        //If we're playing the playlist, play the next song
        else if (PlaylistTracks.Count > 0)
        {
            var nextIndex = PlaylistTracks.IndexOf(currentMedia!) + 1;
            if (nextIndex >= PlaylistTracks.Count)
                nextIndex = 0;
            if (IsShuffle)
                nextIndex = _random.Next(0, PlaylistTracks.Count);

            await _player.LoadTrackAsync(PlaylistTracks[nextIndex]);
            _player.Play();
        }
    }

    public async Task PreviousAsync()
    {
        var lastPlayedTrack = LastPlayedTracks.FirstOrDefault();
        if (lastPlayedTrack is null)
            return;

        var couldLoad = await _player.LoadTrackAsync(lastPlayedTrack);
        if (couldLoad)
            _player.Play();
    }


    #region Playlist Tab Tasks

    public async Task AddVideoToPlaylistAsync()
    {
        if (string.IsNullOrWhiteSpace(AddTrackOrPlaylistUri))
            return;
        var isVideo = VideoId.TryParse(AddTrackOrPlaylistUri);
        var isPlaylist = PlaylistId.TryParse(AddTrackOrPlaylistUri);
        if (isVideo is null && isPlaylist is null)
            return;
        var usePlaylist = (isVideo is not null && isPlaylist is not null && EntirePlaylist) ||
                          isVideo is null && isPlaylist is not null;
        if (usePlaylist)
        {
            var playlist = _client.Playlists.GetVideoBatchesAsync(isPlaylist!.Value);
            await foreach (var batch in playlist)
            {
                foreach (var video in batch.Items)
                {
                    var track = new YouTubeTrack(video.Title, video.Author.ChannelTitle,
                        video.Duration ?? TimeSpan.Zero,
                        video.Thumbnails.GetWithHighestResolution().Url, new Uri(video.Url));
                    PlaylistTracks.Add(track);
                }
            }
        }
        else
        {
            var track = new YouTubeTrack(new Uri("https://www.youtube.com/watch?v=" + isVideo!.Value));
            await track.LoadInfoAsync().ConfigureAwait(true);
            PlaylistTracks.Add(track);
        }

        AddTrackOrPlaylistUri = string.Empty;
    }

    #endregion

    #region Request Tab Tasks

    public async Task AddRequestVideoAsync()
    {
        if (string.IsNullOrWhiteSpace(AddRequestSong))
            return;
        var isVideo = VideoId.TryParse(AddRequestSong);
        if (isVideo is null)
            return;

        var track = new YouTubeTrack(new Uri("https://www.youtube.com/watch?v=" + isVideo!.Value));
        await track.LoadInfoAsync().ConfigureAwait(true);
        RequestTracks.Add(track);
        AddRequestSong = string.Empty;
    }

    #endregion

    #region Search Tab Tasks

    public async Task SearchForTracksAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
            return;
        SearchTracks.Clear();
        var search = _client.Search.GetResultBatchesAsync(SearchQuery, SearchFilter.Video);
        await foreach (var batch in search)
        {
            foreach (var searchResult in batch.Items)
            {
                var video = searchResult as VideoSearchResult;
                if (video is null)
                    continue;

                var track = new YouTubeTrack(video.Title, video.Author.ChannelTitle, video.Duration ?? TimeSpan.Zero,
                    video.Thumbnails.GetWithHighestResolution().Url, new Uri(video.Url));

                SearchTracks.Add(track);
            }

            break;
        }

        SearchQuery = string.Empty;
    }

    #endregion
}