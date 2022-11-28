using APIG.UI.Models;

namespace APIG.UI.EventArgs;

public class TrackStartedEventArgs
{
    public IBaseTrack? Track { get; }

    public TrackStartedEventArgs(IBaseTrack? track)
    {
        Track = track;
    }
}