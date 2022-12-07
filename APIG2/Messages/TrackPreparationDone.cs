using APIG2.Interfaces;

namespace APIG2.Messages;

public class TrackPreparationDone
{
    public IBaseTrack Track { get; }

    public TrackPreparationDone(IBaseTrack track)
    {
        Track = track;
    }
}