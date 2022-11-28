using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace APIG.UI.Controls;

public class MediaControlsControl : TemplatedControl
{
    public static readonly StyledProperty<bool> IsPlayingProperty =
        AvaloniaProperty.Register<MediaControlsControl, bool>(
            nameof(IsPlaying));

    public bool IsPlaying
    {
        get => GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }
    
    public static readonly DirectProperty<MediaControlsControl, bool> IsNotPlayingProperty =
        AvaloniaProperty.RegisterDirect<MediaControlsControl, bool>(
            nameof(IsNotPlaying),
            o => o.IsNotPlaying);
    
    public bool IsNotPlaying => !IsPlaying;

    public static readonly StyledProperty<double> VolumeProperty =
        AvaloniaProperty.Register<MediaControlsControl, double>(
            nameof(Volume));

    public double Volume
    {
        get => GetValue(VolumeProperty);
        set => SetValue(VolumeProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsShuffleProperty = AvaloniaProperty.Register<MediaControlsControl, bool>(
        nameof(IsShuffle));

    public bool IsShuffle
    {
        get => GetValue(IsShuffleProperty);
        set => SetValue(IsShuffleProperty, value);
    }

    public static readonly StyledProperty<bool> SaveToFileProperty = AvaloniaProperty.Register<MediaControlsControl, bool>(
        nameof(SaveToFile));

    public bool SaveToFile
    {
        get => GetValue(SaveToFileProperty);
        set => SetValue(SaveToFileProperty, value);
    }

    //DirectProperty ICommand? RewindCommand
    public static readonly DirectProperty<MediaControlsControl, ICommand?> RewindCommandProperty =
        AvaloniaProperty.RegisterDirect<MediaControlsControl, ICommand?>(
            nameof(RewindCommand),
            o => o.RewindCommand,
            (o, v) => o.RewindCommand = v);

    private ICommand? _rewindCommand;

    public ICommand? RewindCommand
    {
        get => _rewindCommand;
        set => SetAndRaise(RewindCommandProperty, ref _rewindCommand, value);
    }

    //DirectProperty ICommand? PlayCommand
    public static readonly DirectProperty<MediaControlsControl, ICommand?> PlayCommandProperty =
        AvaloniaProperty.RegisterDirect<MediaControlsControl, ICommand?>(
            nameof(PlayCommand),
            o => o.PlayCommand,
            (o, v) => o.PlayCommand = v);

    private ICommand? _playCommand;

    public ICommand? PlayCommand
    {
        get => _playCommand;
        set => SetAndRaise(PlayCommandProperty, ref _playCommand, value);
    }

    //DirectProperty ICommand? PauseCommand
    public static readonly DirectProperty<MediaControlsControl, ICommand?> PauseCommandProperty =
        AvaloniaProperty.RegisterDirect<MediaControlsControl, ICommand?>(
            nameof(PauseCommand),
            o => o.PauseCommand,
            (o, v) => o.PauseCommand = v);

    private ICommand? _pauseCommand;

    public ICommand? PauseCommand
    {
        get => _pauseCommand;
        set => SetAndRaise(PauseCommandProperty, ref _pauseCommand, value);
    }

    //DirectProperty ICommand? StopCommand
    public static readonly DirectProperty<MediaControlsControl, ICommand?> StopCommandProperty =
        AvaloniaProperty.RegisterDirect<MediaControlsControl, ICommand?>(
            nameof(StopCommand),
            o => o.StopCommand,
            (o, v) => o.StopCommand = v);

    private ICommand? _stopCommand;

    public ICommand? StopCommand
    {
        get => _stopCommand;
        set => SetAndRaise(StopCommandProperty, ref _stopCommand, value);
    }
    
    //DirectProperty ICommand? FastForwardCommand
    public static readonly DirectProperty<MediaControlsControl, ICommand?> FastForwardCommandProperty =
        AvaloniaProperty.RegisterDirect<MediaControlsControl, ICommand?>(
            nameof(FastForwardCommand),
            o => o.FastForwardCommand,
            (o, v) => o.FastForwardCommand = v);
    
    private ICommand? _fastForwardCommand;
    
    public ICommand? FastForwardCommand
    {
        get => _fastForwardCommand;
        set => SetAndRaise(FastForwardCommandProperty, ref _fastForwardCommand, value);
    }

    public MediaControlsControl()
    {
        IsPlayingProperty.Changed.Subscribe(_ => RaisePropertyChanged(IsNotPlayingProperty, IsNotPlaying, !IsNotPlaying));
    }
}