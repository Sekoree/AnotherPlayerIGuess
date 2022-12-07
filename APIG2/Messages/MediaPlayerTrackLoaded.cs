using APIG2.Interfaces;

namespace APIG2.Messages;

public class MediaPlayerTrackLoaded
{
    public IBaseTrack Track { get; }

    public MediaPlayerTrackLoaded(IBaseTrack track)
    {
        Track = track;
    }
}