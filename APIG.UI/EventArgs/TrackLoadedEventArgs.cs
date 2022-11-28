using APIG.UI.Models;

namespace APIG.UI.EventArgs;

public class TrackLoadedEventArgs : System.EventArgs
{
    public IBaseTrack Track { get; }

    public TrackLoadedEventArgs(IBaseTrack track)
    {
        Track = track;
    }
}