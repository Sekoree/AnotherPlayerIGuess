using ManagedBass;

namespace APIG2.Messages;

public class MediaPlayerPlaybackStatusChanged
{
    public PlaybackState State { get; }

    public MediaPlayerPlaybackStatusChanged(PlaybackState state)
    {
        State = state;
    }
}