using System;

namespace APIG2.Messages;

public class MediaPlayerTrackPositionChanged
{
    public MediaPlayerTrackPositionChanged(TimeSpan position)
    {
        Position = position;
    }

    public TimeSpan Position { get; }
}