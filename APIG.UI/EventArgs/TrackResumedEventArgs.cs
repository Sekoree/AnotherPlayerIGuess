using APIG.UI.Models;

namespace APIG.UI.EventArgs;

public class TrackResumedEventArgs : System.EventArgs
{
    public IBaseTrack Track { get; }

    public TrackResumedEventArgs(IBaseTrack track)
    {
        Track = track;
    }
}