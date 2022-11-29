using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using APIG.UI.Models;
using APIG.UI.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using ReactiveUI;

namespace APIG.UI.Controls;

[PseudoClasses(":isActiveMedia")]
public class PlaylistItemControl : TemplatedControl
{
    public static readonly StyledProperty<IBaseTrack?> ActiveMediaProperty =
        AvaloniaProperty.Register<PlaylistItemControl, IBaseTrack?>(
            nameof(ActiveMedia));

    public IBaseTrack? ActiveMedia
    {
        get => GetValue(ActiveMediaProperty);
        set => SetValue(ActiveMediaProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<IBaseTrack>> MediaParentCollectionProperty =
        AvaloniaProperty.Register<PlaylistItemControl, ObservableCollection<IBaseTrack>>(
            nameof(MediaParentCollection));

    public ObservableCollection<IBaseTrack> MediaParentCollection
    {
        get => GetValue(MediaParentCollectionProperty);
        set => SetValue(MediaParentCollectionProperty, value);
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
    public static readonly DirectProperty<PlaylistItemControl, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<PlaylistItemControl, string>(
            nameof(Title), o => o.Title);

    public string Title => Media?.Title ?? string.Empty;

    public static readonly DirectProperty<PlaylistItemControl, string> ArtistProperty =
        AvaloniaProperty.RegisterDirect<PlaylistItemControl, string>(
            nameof(Artist), o => o.Artist);

    public string Artist => Media?.Artist ?? string.Empty;

    public static readonly DirectProperty<PlaylistItemControl, TimeSpan> DurationProperty =
        AvaloniaProperty.RegisterDirect<PlaylistItemControl, TimeSpan>(
            nameof(Duration), o => o.Duration);

    public TimeSpan Duration => Media?.Duration ?? TimeSpan.Zero;

    //Timespan string only showing minutes and seconds
    public static readonly DirectProperty<PlaylistItemControl, string> DurationStringProperty =
        AvaloniaProperty.RegisterDirect<PlaylistItemControl, string>(
            nameof(DurationString), o => o.Duration.ToString(@"mm\:ss"), unsetValue: TimeSpan.Zero.ToString(@"mm\:ss"));

    public string DurationString => Duration.ToString(@"mm\:ss") ?? TimeSpan.Zero.ToString(@"mm\:ss");

    public static readonly DirectProperty<PlaylistItemControl, string> AlbumArtUriProperty =
        AvaloniaProperty.RegisterDirect<PlaylistItemControl, string>(
            nameof(AlbumArtUri), o => o.AlbumArtUri);

    public string AlbumArtUri => Media?.AlbumArtUri ?? string.Empty;

    public static readonly StyledProperty<bool> IsActiveMediaProperty =
        AvaloniaProperty.Register<PlaylistItemControl, bool>(
            nameof(IsActiveMedia));

    public bool IsActiveMedia
    {
        get => GetValue(IsActiveMediaProperty);
        set => SetValue(IsActiveMediaProperty, value);
    }

    public static readonly StyledProperty<bool> IsRequestProperty = AvaloniaProperty.Register<PlaylistItemControl, bool>(
        nameof(IsRequest));

    public bool IsRequest
    {
        get => GetValue(IsRequestProperty);
        set => SetValue(IsRequestProperty, value);
    }

    //DirectProperty for ICommand? OpenExternalLinkCommand
    public static readonly DirectProperty<PlaylistItemControl, ICommand?> OpenExternalLinkCommandProperty =
        AvaloniaProperty.RegisterDirect<PlaylistItemControl, ICommand?>(
            nameof(OpenExternalLinkCommand), o => o.OpenExternalLinkCommand);

    private ICommand? _openExternalLinkCommand;

    public ICommand? OpenExternalLinkCommand
    {
        get => _openExternalLinkCommand;
        set => SetAndRaise(OpenExternalLinkCommandProperty, ref _openExternalLinkCommand, value);
    }

    //DirectProperty for ICommand? RemoveMediaCommand
    public static readonly DirectProperty<PlaylistItemControl, ICommand?> RemoveMediaCommandProperty =
        AvaloniaProperty.RegisterDirect<PlaylistItemControl, ICommand?>(
            nameof(RemoveMediaCommand), o => o.RemoveMediaCommand);

    private ICommand? _removeMediaCommand;

    public ICommand? RemoveMediaCommand
    {
        get => _removeMediaCommand;
        set => SetAndRaise(RemoveMediaCommandProperty, ref _removeMediaCommand, value);
    }
    
    private ICommand? _openInBrowserCommand;

    public static readonly DirectProperty<PlaylistItemControl, ICommand?> OpenInBrowserCommandProperty =
        AvaloniaProperty.RegisterDirect<PlaylistItemControl, ICommand?>(
            nameof(OpenInBrowserCommand), o => o.OpenInBrowserCommand, (o, v) => o.OpenInBrowserCommand = v);

    public ICommand? OpenInBrowserCommand
    {
        get => _openInBrowserCommand;
        set => SetAndRaise(OpenInBrowserCommandProperty, ref _openInBrowserCommand, value);
    }

    private ICommand? _copyUrlCommand;

    public static readonly DirectProperty<PlaylistItemControl, ICommand?> CopyUrlCommandProperty =
        AvaloniaProperty.RegisterDirect<PlaylistItemControl, ICommand?>(
            nameof(CopyUrlCommand), o => o.CopyUrlCommand, (o, v) => o.CopyUrlCommand = v);

    public ICommand? CopyUrlCommand
    {
        get => _copyUrlCommand;
        set => SetAndRaise(CopyUrlCommandProperty, ref _copyUrlCommand, value);
    }

    public PlaylistItemControl()
    {
        MediaProperty.Changed.Subscribe(_ =>
        {
            RaisePropertyChanged(TitleProperty, Title, Title);
            RaisePropertyChanged(ArtistProperty, Artist, Artist);
            RaisePropertyChanged(DurationProperty, Duration, Duration);
            RaisePropertyChanged(DurationStringProperty, DurationString, DurationString);
            RaisePropertyChanged(AlbumArtUriProperty, AlbumArtUri, AlbumArtUri);
        });
        ActiveMediaProperty.Changed.Subscribe(_ => { IsActiveMedia = ActiveMedia == Media; });
        IsActiveMediaProperty.Changed.Subscribe(_ =>
        {
            PseudoClasses.Set(":isActiveMedia", IsActiveMedia);
            Classes.Set("isActiveMedia", IsActiveMedia);
        });

        OpenExternalLinkCommand = ReactiveCommand.Create(() =>
        {
            if (Media is not null)
                Process.Start(new ProcessStartInfo(Media.Source.ToString()) { UseShellExecute = true });
        });
        
        RemoveMediaCommand = ReactiveCommand.Create(() =>
        {
            if (Media is not null)
                MediaParentCollection.Remove(Media);
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
        
        this.IsHitTestVisible = true;
        this.DoubleTapped += (sender, args) =>
        {
            //get parent window
            var window = this.VisualRoot as Window;
            if (window is null || Media is null || IsRequest)
                return;
            
            var windowViewModel = window.DataContext as MainWindowViewModel;
            if (windowViewModel is null)
                return;
            
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await windowViewModel.PlaySpecificTrackAsync(Media);
            });
        };
    }
}