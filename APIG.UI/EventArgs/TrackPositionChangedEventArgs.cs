using System;

namespace APIG.UI.EventArgs;

public class TrackPositionChangedEventArgs
{
    public TimeSpan Position { get; }
    
    public TrackPositionChangedEventArgs(TimeSpan position)
    {
        Position = position;
    }
}