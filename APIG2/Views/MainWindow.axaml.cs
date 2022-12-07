using APIG2.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace APIG2.Views;

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
        Dispatcher.UIThread.Post(() => dc.AddTracksToPlaylistCommand.Execute(null));
    }

    private void Requests_TextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) 
            return;
        
        var dc = (MainWindowViewModel) DataContext!;
        Dispatcher.UIThread.Post(() => dc.AddTrackToRequestsCommand.Execute(null));
    }

    private void Search_TextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) 
            return;
        
        var dc = (MainWindowViewModel) DataContext!;
        Dispatcher.UIThread.Post(() => dc.SearchCommand.Execute(null));
    }
}