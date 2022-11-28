using APIG.UI.Models;

namespace APIG.UI.EventArgs;

public class TrackFinishedEventArgs : System.EventArgs
{
    public IBaseTrack? Track { get; }

    public TrackFinishedEventArgs(IBaseTrack? track)
    {
        Track = track;
    }
}