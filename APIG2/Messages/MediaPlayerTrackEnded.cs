using APIG2.Interfaces;

namespace APIG2.Messages;

public class MediaPlayerTrackEnded
{
    public IBaseTrack Track { get; }

    public MediaPlayerTrackEnded(IBaseTrack track)
    {
        Track = track;
    }
}