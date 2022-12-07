using APIG2.Interfaces;

namespace APIG2.Messages;

public class TrackPreparationSlow
{
    public IBaseTrack Track { get; }

    public TrackPreparationSlow(IBaseTrack track)
    {
        Track = track;
    }
}