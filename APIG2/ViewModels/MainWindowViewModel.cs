using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using APIG2.Interfaces;
using APIG2.Messages;
using APIG2.Models;
using APIG2.Models.Json;
using APIG2.Twitch;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ManagedBass;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace APIG2.ViewModels;

[INotifyPropertyChanged]
public partial class MainWindowViewModel
{
    private YoutubeClient _youtube = new();
    private MediaPlayer _mediaPlayer = new();
    private Random _random = new();
    private Bot _twitchBot = new();

    #region Sidebar Info

    [ObservableProperty] private IBaseTrack? _currentTrack;
    [ObservableProperty] private IBaseTrack? _nextTrack;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    private bool _isPlaying;

    [ObservableProperty] private TimeSpan _currentPosition;

    [ObservableProperty] private bool _showProgress = false;
    [ObservableProperty] private double _prepProgress = 0;

    #endregion

    #region Sidebar Controls

    private bool _isShuffleEnabled;
    public bool IsShuffleEnabled
    {
        get => _isShuffleEnabled;
        set
        {
            _isShuffleEnabled = value;
            OnPropertyChanged();
            if (PlaylistTracks.Count == 1) 
                return;
            if (NextTrack != null)
                _ = Task.Run(NextTrack.UnprepareTrackAsync);
            NextTrack = GetNextTrack();
        }
    }

    [ObservableProperty] private bool _isSaveToFileEnabled;

    public float Volume
    {
        get => (float)Math.Pow(_mediaPlayer.Volume, 1 / 4.0);
        set
        {
            _mediaPlayer.Volume = (float)Math.Pow(value, 4);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Volume)));
        }
    }

    #endregion

    #region Playlist Tab

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddTracksToPlaylistCommand))]
    private string _videoOrPlaylistUrl = string.Empty;

    [ObservableProperty] private bool _useFullPlaylist;
    [ObservableProperty] private ObservableCollection<IBaseTrack> _playlistTracks = new();
    [ObservableProperty] private ObservableCollection<IBaseTrack> _lastPlayedTracks = new();

    private bool VideoOrPlaylistUrlIsValid => VideoId.TryParse(VideoOrPlaylistUrl) != null ||
                                              PlaylistId.TryParse(VideoOrPlaylistUrl) != null;

    private bool CanPlay => PlaylistTracks.Count > 0;

    private bool CanStop => IsPlaying;

    #endregion

    #region Requests Tab

    [ObservableProperty] private ObservableCollection<IBaseTrack> _requestTracks = new();

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddTrackToRequestsCommand))]
    private string _requestToAddUrl = string.Empty;

    private bool RequestVideoUrlIsValid => VideoId.TryParse(RequestToAddUrl) != null;

    #endregion

    #region Search Tab

    [ObservableProperty] private ObservableCollection<IBaseTrack> _searchTracks = new();

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private string _searchQuery = string.Empty;

    private bool SearchQueryIsValid => !string.IsNullOrWhiteSpace(SearchQuery);

    #endregion

    #region Visualizer Tab

    [ObservableProperty] private float[] _currentFftData = Array.Empty<float>();
    [ObservableProperty] private float[] _currentMaxData = Array.Empty<float>();

    #endregion

    #region Settings Tab

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(FillChannelRewardsCommand), nameof(ConnectToChatCommand))]
    private string _username = string.Empty;

    [ObservableProperty] private ObservableCollection<CustomReward> _customRewards = new();

    private CustomReward? _selectedReward;

    public CustomReward? SelectedReward
    {
        get => _selectedReward;
        set
        {
            if (Equals(value, _selectedReward))
                return;
            _selectedReward = value;
            _twitchBot.SelectedReward = value;
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectToChatCommand))]
    [NotifyCanExecuteChangedFor(nameof(DisonnectFromChatCommand))]
    private bool _botConnected = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectToChatCommand))]
    [NotifyPropertyChangedFor(nameof(CanConnect))]
    private string _chatToken = string.Empty;

    [ObservableProperty] private string _lastStatus = "Nothing right now";

    public bool CanConnect =>
        !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(ChatToken) && (!BotConnected);

    private bool CanDisconnect => BotConnected;
    private bool CanGetRewards => !string.IsNullOrWhiteSpace(Username);

    #endregion

    public MainWindowViewModel()
    {
        InitErrorMessages();
        InitDataChangedMessages();

        App.Messenger.Register<TwitchBotRequestReceived>(this, (sender, message) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                RequestToAddUrl = message.VideoUrl;
                AddTrackToRequestsCommand.Execute(null);
            });
        });
        App.Messenger.Register<TwitchBotConnectionMessage>(this, (sender, message) =>
        {
            switch (message.Status)
            {
                case TwitchBotConnectionStatus.Connected:
                    Dispatcher.UIThread.Post(() =>
                    {
                        LastStatus = "Connected to chat";
                        BotConnected = true;
                    });
                    break;
                case TwitchBotConnectionStatus.Disconnected:
                    Dispatcher.UIThread.Post(() =>
                    {
                        LastStatus = "Disconnected from chat";
                        BotConnected = false;
                    });
                    break;
                case TwitchBotConnectionStatus.Error:
                    Dispatcher.UIThread.Post(() =>
                    {
                        LastStatus = "Error connecting to chat: " + message.Message;
                        BotConnected = false;
                    });
                    break;
            }
        });

        PlaylistTracks.CollectionChanged += (_, _)
            =>
        {
            PlayCommand.NotifyCanExecuteChanged();
            NextCommand.NotifyCanExecuteChanged();
            PreviousCommand.NotifyCanExecuteChanged();
            if (NextTrack != null && NextTrack == CurrentTrack)
                _ = Task.Run(NextTrack!.UnprepareTrackAsync);
            if (CurrentTrack != null && CurrentTrack == NextTrack)
                NextTrack = GetNextTrack();
        };
        LastPlayedTracks.CollectionChanged += (_, _)
            => PreviousCommand.NotifyCanExecuteChanged();
        RequestTracks.CollectionChanged += (_, _)
            =>
        {
            if (NextTrack != null && NextTrack != RequestTracks.FirstOrDefault())
                _ = Task.Run(NextTrack!.UnprepareTrackAsync);
            if (CurrentTrack != null && NextTrack != RequestTracks.FirstOrDefault())
                NextTrack = GetNextTrack();
        };
        _mediaPlayer.Init();
    }

    private void InitErrorMessages()
    {
        App.Messenger.Register<MainViewModelError>(this,
            (sender, msg) =>
            {
                File.AppendAllText("error.log", $"[{DateTime.Now}] [MainVM] {msg.Message}{Environment.NewLine}");
            });
        App.Messenger.Register<MediaPlayerError>(this, (sender, msg) =>
        {
            File.AppendAllText("error.log", $"[{DateTime.Now}] [MediaPlayer] {msg.Message}{Environment.NewLine}" +
                                            $"- Bass Error: {msg.Exception}{Environment.NewLine}");
        });
        App.Messenger.Register<TrackError>(this, (sender, msg) =>
        {
            File.AppendAllText("error.log", $"[{DateTime.Now}] [Track] {msg.Message}{Environment.NewLine}" +
                                            $"- Exception: {msg.Exception}{Environment.NewLine}");
        });
        App.Messenger.Register<TrackPrepareError>(this, (sender, msg) =>
        {
            File.AppendAllText("error.log", $"[{DateTime.Now}] [Playlist] {msg.Message}{Environment.NewLine}" +
                                            $"- Exception: {msg.Exception}{Environment.NewLine}" +
                                            $"- Bass Error: {msg.BassError}{Environment.NewLine}");
            Dispatcher.UIThread.Post(() => NextCommand.Execute(null));
        });
    }

    private void InitDataChangedMessages()
    {
        App.Messenger.Register<MediaPlayerTrackPositionChanged>(this, (sender, msg) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (CurrentPosition.Seconds != msg.Position.Seconds)
                    CurrentPosition = msg.Position;
            });
        });
        App.Messenger.Register<MediaPlayerTrackFftRendered>(this,
            (sender, msg) => { Dispatcher.UIThread.Post(() => CurrentFftData = msg.FftData); });
        App.Messenger.Register<MediaPlayerTrackLoaded>(this, (sender, msg) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                CurrentMaxData = msg.Track.FftMaxPerSegment;
                CurrentTrack = msg.Track;
                NextTrack = GetNextTrack();
            });
        });
        App.Messenger.Register<MediaPlayerPlaybackStatusChanged>(this, (sender, msg) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                switch (msg.State)
                {
                    case PlaybackState.Playing:
                        IsPlaying = true;
                        if (!IsSaveToFileEnabled)
                            break;
                        File.WriteAllText("Title.txt", CurrentTrack!.Title);
                        File.WriteAllText("Artist.txt", CurrentTrack.Artist);
                        break;
                    case PlaybackState.Paused:
                        IsPlaying = false;
                        break;
                    case PlaybackState.Stopped:
                        IsPlaying = false;
                        CurrentTrack = null;
                        NextTrack = null;
                        CurrentFftData = Array.Empty<float>();
                        break;
                    default:
                        return;
                }
            });
        });
        App.Messenger.Register<MediaPlayerTrackEnded>(this, (sender, msg) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (PlaylistTracks.Count == 0)
                    StopCommand.Execute(null);
                else
                    NextCommand.Execute(null);
            });
        });
        
        App.Messenger.Register<TrackPreparationSlow>(this, (recipient, message) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (CurrentTrack != null) 
                    return;
                ShowProgress = true;
                PrepProgress = message.Track.StreamPercentLoaded;
            });
        });
        App.Messenger.Register<TrackPreparationDone>(this, (recipient, message) =>
        {
            if (CurrentTrack != null) 
                return;
            ShowProgress = false;
            PrepProgress = 0;
        });
    }

    #region Playlist Tab Commands

    [RelayCommand(CanExecute = nameof(VideoOrPlaylistUrlIsValid))]
    public async Task AddTracksToPlaylist()
    {
        try
        {
            var videoId = VideoId.TryParse(VideoOrPlaylistUrl);
            var playlistId = PlaylistId.TryParse(VideoOrPlaylistUrl);

            if (videoId is not null && (playlistId is null || !UseFullPlaylist))
            {
                var video = new YouTubeTrack(VideoOrPlaylistUrl);
                var couldLoad = await video.LoadInfoAsync();
                if (!couldLoad)
                    return;

                PlaylistTracks.Add(video);
            }
            else if (playlistId is not null)
            {
                var playlist = _youtube.Playlists.GetVideoBatchesAsync(playlistId!.Value);
                await foreach (var batch in playlist)
                {
                    foreach (var video in batch.Items)
                    {
                        var thumbnail = video.Thumbnails.GetWithHighestResolution();
                        var track = new YouTubeTrack(video.Url, video.Title, video.Author.ChannelTitle,
                            video.Duration ?? TimeSpan.Zero, thumbnail.Url);
                        PlaylistTracks.Add(track);
                    }
                }
            }
            else
            {
                App.Messenger.Send(new MainViewModelError("Invalid URL"));
                VideoOrPlaylistUrl = "Invalid URL";
                return;
            }

            VideoOrPlaylistUrl = string.Empty;
        }
        catch (Exception e)
        {
            await File.AppendAllTextAsync("error.log", $"[{DateTime.Now}] [AddTracksToPlaylist] {e}{Environment.NewLine}");
        }
    }

    #endregion

    #region Requests Tab Commands

    [RelayCommand(CanExecute = nameof(RequestVideoUrlIsValid))]
    public async Task AddTrackToRequests()
    {
        var video = new YouTubeTrack(RequestToAddUrl);
        var couldLoad = await video.LoadInfoAsync();
        if (!couldLoad)
            return;

        RequestTracks.Add(video);
        RequestToAddUrl = string.Empty;
    }

    #endregion

    #region Search Tab Commands

    [RelayCommand(CanExecute = nameof(SearchQueryIsValid))]
    public async Task Search()
    {
        var search = _youtube.Search.GetResultBatchesAsync(SearchQuery, SearchFilter.Video);
        await foreach (var batch in search)
        {
            foreach (var videos in batch.Items)
            {
                if (videos is not VideoSearchResult asVideo)
                    continue;
                var track = new YouTubeTrack(asVideo.Url, asVideo.Title, asVideo.Author.ChannelTitle,
                    asVideo.Duration ?? TimeSpan.Zero, asVideo.Thumbnails.GetWithHighestResolution().Url);
                SearchTracks.Add(track);
            }

            // Only get the first page of results
            break;
        }

        SearchQuery = string.Empty;
    }

    #endregion

    #region Settings Tab Commands

    [RelayCommand(CanExecute = nameof(CanGetRewards))]
    public async Task FillChannelRewards()
    {
        CustomRewards.Clear();
        var rewards = await _twitchBot.GetChannelPointRewardsAsync(Username);
        if (rewards is null)
            return;
        foreach (var reward in rewards.Data.Community.Channel.CommunityPointsSettings.CustomRewards)
            CustomRewards.Add(reward);
        SelectedReward = CustomRewards.FirstOrDefault();
    }

    [RelayCommand(CanExecute = nameof(CanConnect))]
    public async Task ConnectToChat()
    {
        _twitchBot.Username = Username;
        _twitchBot.Token = ChatToken;
        _twitchBot.SelectedReward = SelectedReward;
        await _twitchBot.ConnectAsync();
    }

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    public void DisonnectFromChat()
    {
        _twitchBot.Disconnect();
    }

    [RelayCommand]
    public async Task GetChatToken()
    {
        var url =
            "https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=o22ihma1s9bhq5znjyyv8lh70ny4gr&redirect_uri=http%3A%2F%2Flocalhost%3A8888&scope=chat:read%20chat:edit";
        var token = await AuthFlow.DoOAuthFlowAsync(url);
        ChatToken = token;
    }

    #endregion

    #region General Controls

    [RelayCommand(CanExecute = nameof(CanPlay))]
    public async Task Play()
    {
        switch (_mediaPlayer.State)
        {
            case PlaybackState.Paused:
                _mediaPlayer.Resume();
                break;
            default:
            {
                if (PlaylistTracks.Count == 0)
                {
                    App.Messenger.Send(new MainViewModelError("No tracks to play"));
                    return;
                }

                var track = PlaylistTracks[0];
                await PlaySpecificTrack(track);
                break;
            }
        }
    }

    public async Task PlaySpecificTrack(IBaseTrack track)
    {
        _mediaPlayer.Stop();
        if (track.PrepStatus == PreparationStatus.NotPrepared)
        {
            await track.PrepareTrackAsync();
        }

        var couldLoad = await _mediaPlayer.LoadTrackAsync(track);
        if (!couldLoad)
            return;

        _mediaPlayer.Play();
    }

    public IBaseTrack? GetNextTrack()
    {
        if (CurrentTrack != null && PlaylistTracks.Contains(CurrentTrack))
            LastPlayedTracks.Add(CurrentTrack);

        if (RequestTracks.Count > 0)
            return RequestTracks[0];

        if (PlaylistTracks.Count == 0)
        {
            stopCommand!.Execute(null);
            return default;
        }

        var currentTrackIndex = CurrentTrack == null ? 0 : PlaylistTracks.IndexOf(CurrentTrack);
        var nextTrackIndex = (currentTrackIndex + 1) >= PlaylistTracks.Count ? 0 : currentTrackIndex + 1;
        if (IsShuffleEnabled)
            nextTrackIndex = _random.Next(0, PlaylistTracks.Count);

        var track = PlaylistTracks[nextTrackIndex];
        if (track.PrepStatus == PreparationStatus.NotPrepared)
            _ = Task.Run(track.PrepareTrackAsync);

        return PlaylistTracks[nextTrackIndex];
    }

    [RelayCommand]
    public void Pause() => _mediaPlayer.Pause();

    [RelayCommand(CanExecute = nameof(CanStop))]
    public async Task Stop()
    {
        if (CurrentTrack != null && RequestTracks.Contains(CurrentTrack))
            RequestTracks.Remove(CurrentTrack);
        if (CurrentTrack != null)
            await CurrentTrack.UnprepareTrackAsync();
        _mediaPlayer.Stop();
    }

    [RelayCommand(CanExecute = nameof(CanPlay))]
    public async Task Next()
    {
        if (CurrentTrack != null && RequestTracks.Contains(CurrentTrack))
            RequestTracks.Remove(CurrentTrack);
        if (CurrentTrack != null && CurrentTrack != NextTrack)
            await CurrentTrack.UnprepareTrackAsync();
        await PlaySpecificTrack(NextTrack!);
    }

    [RelayCommand(CanExecute = nameof(CanPlay))]
    public async Task Previous()
    {
        if (LastPlayedTracks.Count == 0)
            await PlaySpecificTrack(CurrentTrack!);
        else
            await PlaySpecificTrack(LastPlayedTracks[0]);
    }

    #endregion
}