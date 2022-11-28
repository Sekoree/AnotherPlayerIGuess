using APIG.UI.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace APIG.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Playlist_TextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) 
            return;
        
        var dc = (MainWindowViewModel) DataContext!;
        Dispatcher.UIThread.InvokeAsync(async () => await dc.AddVideoToPlaylistAsync());
    }

    private void Requests_TextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) 
            return;
        
        var dc = (MainWindowViewModel) DataContext!;
        Dispatcher.UIThread.InvokeAsync(async () => await dc.AddRequestVideoAsync());
    }

    private void Search_TextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) 
            return;
        
        var dc = (MainWindowViewModel) DataContext!;
        Dispatcher.UIThread.InvokeAsync(async () => await dc.SearchForTracksAsync());
    }
}