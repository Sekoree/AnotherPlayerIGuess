using APIG.UI.Models;

namespace APIG.UI.EventArgs;

public class TrackPausedEventArgs
{
    public IBaseTrack Track { get; }

    public TrackPausedEventArgs(IBaseTrack track)
    {
        Track = track;
    }
}