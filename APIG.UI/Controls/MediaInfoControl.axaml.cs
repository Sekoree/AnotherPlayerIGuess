using System;
using APIG.UI.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace APIG.UI.Controls;

[PseudoClasses(":isPlaying")]
public class MediaInfoControl : TemplatedControl
{
    public static readonly StyledProperty<TimeSpan> CurrentPositionProperty =
        AvaloniaProperty.Register<MediaInfoControl, TimeSpan>(
            nameof(CurrentPosition), TimeSpan.Zero);

    public TimeSpan CurrentPosition
    {
        get => GetValue(CurrentPositionProperty);
        set => SetValue(CurrentPositionProperty, value);
    }
    
    public static readonly DirectProperty<MediaInfoControl, string> CurrentPositionStringProperty =
        AvaloniaProperty.RegisterDirect<MediaInfoControl, string>(
            nameof(CurrentPositionString), o => o.CurrentPosition.ToString(@"mm\:ss"), unsetValue: TimeSpan.Zero.ToString(@"mm\:ss"));

    public string CurrentPositionString => CurrentPosition.ToString(@"mm\:ss") ?? TimeSpan.Zero.ToString(@"mm\:ss");
    
    public static readonly DirectProperty<MediaInfoControl, double> CurrentPositionSecondsProperty =
        AvaloniaProperty.RegisterDirect<MediaInfoControl, double>(
            nameof(CurrentPositionSeconds), o => o.CurrentPosition.TotalSeconds, unsetValue: 0.0);

    public double CurrentPositionSeconds => CurrentPosition.TotalSeconds;

    public static readonly StyledProperty<IBaseTrack?> CurrentMediaProperty =
        AvaloniaProperty.Register<MediaInfoControl, IBaseTrack?>(
            nameof(CurrentMedia));

    public IBaseTrack? CurrentMedia
    {
        get => GetValue(CurrentMediaProperty);
        set => SetValue(CurrentMediaProperty, value);
    }

    public static readonly StyledProperty<bool> IsPlayingProperty = AvaloniaProperty.Register<MediaInfoControl, bool>(
        nameof(IsPlaying), false);

    public bool IsPlaying
    {
        get => GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    //readonly DirectProperties for Media.Title, Media.Artist, Media.Duration
    public static readonly DirectProperty<MediaInfoControl, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<MediaInfoControl, string>(
            nameof(Title), o => o.Title);

    public string Title => CurrentMedia?.Title ?? string.Empty;
    
    public static readonly DirectProperty<MediaInfoControl, string> ArtistProperty =
        AvaloniaProperty.RegisterDirect<MediaInfoControl, string>(
            nameof(Artist), o => o.Artist);
    
    public string Artist => CurrentMedia?.Artist ?? string.Empty;
    
    public static readonly DirectProperty<MediaInfoControl, TimeSpan> DurationProperty =
        AvaloniaProperty.RegisterDirect<MediaInfoControl, TimeSpan>(
            nameof(Duration), o => o.Duration);
    
    public TimeSpan Duration => CurrentMedia?.Duration ?? TimeSpan.Zero;
    
    public static readonly DirectProperty<MediaInfoControl, string> DurationStringProperty =
        AvaloniaProperty.RegisterDirect<MediaInfoControl, string>(
            nameof(DurationString), o => o.Duration.ToString(@"mm\:ss"), unsetValue: TimeSpan.Zero.ToString(@"mm\:ss"));

    public string DurationString => Duration.ToString(@"mm\:ss") ?? TimeSpan.Zero.ToString(@"mm\:ss");
    
    public static readonly DirectProperty<MediaInfoControl, double> DurationSecondsProperty =
        AvaloniaProperty.RegisterDirect<MediaInfoControl, double>(
            nameof(DurationSeconds), o => o.Duration.TotalSeconds, unsetValue: 0.0);

    public double DurationSeconds => Duration.TotalSeconds;
    
    public static readonly DirectProperty<MediaInfoControl, string> AlbumArtUriProperty =
        AvaloniaProperty.RegisterDirect<MediaInfoControl, string>(
            nameof(AlbumArtUri), o => o.AlbumArtUri);
    
    public string AlbumArtUri => CurrentMedia?.AlbumArtUri ?? string.Empty;
    
    public MediaInfoControl()
    {
        PseudoClasses.Set(":isPlaying", IsPlaying);
        CurrentMediaProperty.Changed.AddClassHandler<MediaInfoControl>((o, e) =>
        {
            RaisePropertyChanged(TitleProperty, Title, Title);
            RaisePropertyChanged(ArtistProperty, Artist, Artist);
            RaisePropertyChanged(DurationProperty, Duration, Duration);
            RaisePropertyChanged(DurationStringProperty, DurationString, DurationString);
            RaisePropertyChanged(DurationSecondsProperty, DurationSeconds, DurationSeconds);
            RaisePropertyChanged(AlbumArtUriProperty, AlbumArtUri, AlbumArtUri);
        });
        CurrentPositionProperty.Changed.AddClassHandler<MediaInfoControl>((o, e) =>
        {
            RaisePropertyChanged(CurrentPositionStringProperty, CurrentPositionString, CurrentPositionString);
            RaisePropertyChanged(CurrentPositionSecondsProperty, CurrentPositionSeconds, CurrentPositionSeconds);
        });
    }
}