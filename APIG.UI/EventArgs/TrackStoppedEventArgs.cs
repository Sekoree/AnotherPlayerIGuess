using APIG.UI.Models;

namespace APIG.UI.EventArgs;

public class TrackStoppedEventArgs : System.EventArgs
{
    public IBaseTrack Track { get; }

    public TrackStoppedEventArgs(IBaseTrack track)
    {
        Track = track;
    }
}