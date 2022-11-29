using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using APIG.UI.Models;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using ReactiveUI;
using YoutubeExplode.Videos;

namespace APIG.UI.Controls;

public class SearchResultItemControl : TemplatedControl
{
    public static readonly StyledProperty<ObservableCollection<IBaseTrack>> PlaylistProperty =
        AvaloniaProperty.Register<SearchResultItemControl, ObservableCollection<IBaseTrack>>(
            nameof(Playlist));

    public ObservableCollection<IBaseTrack> Playlist
    {
        get => GetValue(PlaylistProperty);
        set => SetValue(PlaylistProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<IBaseTrack>> RequestsProperty =
        AvaloniaProperty.Register<SearchResultItemControl, ObservableCollection<IBaseTrack>>(
            nameof(Requests));

    public ObservableCollection<IBaseTrack> Requests
    {
        get => GetValue(RequestsProperty);
        set => SetValue(RequestsProperty, value);
    }

    public static readonly StyledProperty<IBaseTrack?> MediaProperty =
        AvaloniaProperty.Register<PlaylistItemControl, IBaseTrack?>(
            nameof(Media));

    public IBaseTrack? Media
    {
        get => GetValue(MediaProperty);
        set => SetValue(MediaProperty, value);
    }

    //readonly DirectProperties for Media.Title, Media.Artist, Media.Duration
    public static readonly DirectProperty<SearchResultItemControl, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<SearchResultItemControl, string>(
            nameof(Title), o => o.Title);

    public string Title => Media?.Title ?? string.Empty;

    public static readonly DirectProperty<SearchResultItemControl, string> ArtistProperty =
        AvaloniaProperty.RegisterDirect<SearchResultItemControl, string>(
            nameof(Artist), o => o.Artist);

    public string Artist => Media?.Artist ?? string.Empty;

    public static readonly DirectProperty<SearchResultItemControl, TimeSpan> DurationProperty =
        AvaloniaProperty.RegisterDirect<SearchResultItemControl, TimeSpan>(
            nameof(Duration), o => o.Duration);

    public TimeSpan Duration => Media?.Duration ?? TimeSpan.Zero;

    public static readonly DirectProperty<SearchResultItemControl, string> DurationStringProperty =
        AvaloniaProperty.RegisterDirect<SearchResultItemControl, string>(
            nameof(DurationString), o => o.Duration.ToString(@"mm\:ss"), unsetValue: TimeSpan.Zero.ToString(@"mm\:ss"));

    public string DurationString => Duration.ToString(@"mm\:ss") ?? TimeSpan.Zero.ToString(@"mm\:ss");

    public static readonly DirectProperty<SearchResultItemControl, string> AlbumArtUriProperty =
        AvaloniaProperty.RegisterDirect<SearchResultItemControl, string>(
            nameof(AlbumArtUri), o => o.AlbumArtUri);

    public string AlbumArtUri => Media?.AlbumArtUri ?? string.Empty;

    //DirectProperty for ICommand? OpenExternalLinkCommand
    public static readonly DirectProperty<SearchResultItemControl, ICommand?> OpenExternalLinkCommandProperty =
        AvaloniaProperty.RegisterDirect<SearchResultItemControl, ICommand?>(
            nameof(OpenExternalLinkCommand), o => o.OpenExternalLinkCommand);

    private ICommand? _openExternalLinkCommand;

    public ICommand? OpenExternalLinkCommand
    {
        get => _openExternalLinkCommand;
        set => SetAndRaise(OpenExternalLinkCommandProperty, ref _openExternalLinkCommand, value);
    }

    //DirectProperty for ICommand? AddToPlaylistCommand
    public static readonly DirectProperty<SearchResultItemControl, ICommand?> AddToPlaylistCommandProperty =
        AvaloniaProperty.RegisterDirect<SearchResultItemControl, ICommand?>(
            nameof(AddToPlaylistCommand), o => o.AddToPlaylistCommand);

    private ICommand? _addToPlaylistCommand;

    public ICommand? AddToPlaylistCommand
    {
        get => _addToPlaylistCommand;
        set => SetAndRaise(AddToPlaylistCommandProperty, ref _addToPlaylistCommand, value);
    }

    //DirectProperty for ICommand? AddToRequestQueueCommand
    public static readonly DirectProperty<SearchResultItemControl, ICommand?> AddToRequestQueueCommandProperty =
        AvaloniaProperty.RegisterDirect<SearchResultItemControl, ICommand?>(
            nameof(AddToRequestQueueCommand), o => o.AddToRequestQueueCommand);

    private ICommand? _addToRequestQueueCommand;

    public ICommand? AddToRequestQueueCommand
    {
        get => _addToRequestQueueCommand;
        set => SetAndRaise(AddToRequestQueueCommandProperty, ref _addToRequestQueueCommand, value);
    }

    public static readonly StyledProperty<bool> IsInPlaylistProperty =
        AvaloniaProperty.Register<SearchResultItemControl, bool>(
            nameof(IsInPlaylist));

    public bool IsInPlaylist
    {
        get => GetValue(IsInPlaylistProperty);
        set => SetValue(IsInPlaylistProperty, value);
    }

    //DirectProperty bool IsNotInPlaylist
    public static readonly DirectProperty<SearchResultItemControl, bool> IsNotInPlaylistProperty =
        AvaloniaProperty.RegisterDirect<SearchResultItemControl, bool>(
            nameof(IsNotInPlaylist), o => o.IsNotInPlaylist);

    public bool IsNotInPlaylist => !IsInPlaylist;

    public static readonly StyledProperty<bool> IsInRequestsProperty =
        AvaloniaProperty.Register<SearchResultItemControl, bool>(
            nameof(IsInRequests));

    public bool IsInRequests
    {
        get => GetValue(IsInRequestsProperty);
        set => SetValue(IsInRequestsProperty, value);
    }

    //DirectProperty bool IsNotInRequests
    public static readonly DirectProperty<SearchResultItemControl, bool> IsNotInRequestsProperty =
        AvaloniaProperty.RegisterDirect<SearchResultItemControl, bool>(
            nameof(IsNotInRequests), o => o.IsNotInRequests);

    public bool IsNotInRequests => !IsInRequests;
    
    private ICommand? _openInBrowserCommand;

    public static readonly DirectProperty<SearchResultItemControl, ICommand?> OpenInBrowserCommandProperty =
        AvaloniaProperty.RegisterDirect<SearchResultItemControl, ICommand?>(
            nameof(OpenInBrowserCommand), o => o.OpenInBrowserCommand, (o, v) => o.OpenInBrowserCommand = v);

    public ICommand? OpenInBrowserCommand
    {
        get => _openInBrowserCommand;
        set => SetAndRaise(OpenInBrowserCommandProperty, ref _openInBrowserCommand, value);
    }

    private ICommand? _copyUrlCommand;

    public static readonly DirectProperty<SearchResultItemControl, ICommand?> CopyUrlCommandProperty =
        AvaloniaProperty.RegisterDirect<SearchResultItemControl, ICommand?>(
            nameof(CopyUrlCommand), o => o.CopyUrlCommand, (o, v) => o.CopyUrlCommand = v);

    public ICommand? CopyUrlCommand
    {
        get => _copyUrlCommand;
        set => SetAndRaise(CopyUrlCommandProperty, ref _copyUrlCommand, value);
    }

    public SearchResultItemControl()
    {
        MediaProperty.Changed.Subscribe(_ =>
        {
            RaisePropertyChanged(TitleProperty, Title, Title);
            RaisePropertyChanged(ArtistProperty, Artist, Artist);
            RaisePropertyChanged(DurationProperty, Duration, Duration);
            RaisePropertyChanged(DurationStringProperty, DurationString, DurationString);
            RaisePropertyChanged(AlbumArtUriProperty, AlbumArtUri, AlbumArtUri);

            IsInPlaylist = Playlist?.Any(x =>
                VideoId.TryParse(x.Source.ToString()) == VideoId.TryParse(Media?.Source.ToString())) ?? false;
            IsInRequests = Requests?.Any(x =>
                VideoId.TryParse(x.Source.ToString()) == VideoId.TryParse(Media?.Source.ToString())) ?? false;
            RaisePropertyChanged(IsNotInPlaylistProperty, IsNotInPlaylist, IsNotInPlaylist);
            RaisePropertyChanged(IsNotInRequestsProperty, IsNotInRequests, IsNotInRequests);
        });

        PlaylistProperty.Changed.Subscribe(_ =>
        {
            IsInPlaylist = Playlist?.Any(x =>
                VideoId.TryParse(x.Source.ToString()) == VideoId.TryParse(Media?.Source.ToString())) ?? false;
            
            Playlist!.CollectionChanged += (sender, args) =>
            {
                IsInPlaylist = Playlist?.Any(x =>
                    VideoId.TryParse(x.Source.ToString()) == VideoId.TryParse(Media?.Source.ToString())) ?? false;
                RaisePropertyChanged(IsNotInPlaylistProperty, IsNotInPlaylist, IsNotInPlaylist);
            };

            RaisePropertyChanged(IsNotInPlaylistProperty, IsNotInPlaylist, IsNotInPlaylist);
        });

        RequestsProperty.Changed.Subscribe(_ =>
        {
            IsInRequests = Requests?.Any(x =>
                VideoId.TryParse(x.Source.ToString()) == VideoId.TryParse(Media?.Source.ToString())) ?? false;
            
            Requests!.CollectionChanged += (sender, args) =>
            {
                IsInRequests = Requests?.Any(x =>
                    VideoId.TryParse(x.Source.ToString()) == VideoId.TryParse(Media?.Source.ToString())) ?? false;
                RaisePropertyChanged(IsNotInRequestsProperty, IsNotInRequests, IsNotInRequests);
            };


            RaisePropertyChanged(IsNotInRequestsProperty, IsNotInRequests, IsNotInRequests);
        });

        OpenExternalLinkCommand = ReactiveCommand.Create(() =>
        {
            if (Media is not null)
                Process.Start(new ProcessStartInfo(Media.Source.ToString()) { UseShellExecute = true });
        });

        AddToPlaylistCommand = ReactiveCommand.Create(() =>
        {
            if (Media is not null)
                Playlist.Add(new YouTubeTrack(Media));
        });

        AddToRequestQueueCommand = ReactiveCommand.Create(() =>
        {
            if (Media is not null)
                Requests.Add(new YouTubeTrack(Media));
        });
        
        OpenInBrowserCommand = ReactiveCommand.Create(() =>
        {
            if (Media is null)
                return;
            Process.Start(new ProcessStartInfo(Media.Source.ToString())
            {
                UseShellExecute = true
            });
        });

        CopyUrlCommand = ReactiveCommand.Create(() =>
        {
            if (Media is null)
                return;
            Dispatcher.UIThread.InvokeAsync(async () =>
                await Application.Current!.Clipboard!.SetTextAsync(Media.Source.ToString()));
        });
    }
}