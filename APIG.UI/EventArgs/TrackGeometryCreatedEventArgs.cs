using Avalonia.Media;

namespace APIG.UI.EventArgs;

public class TrackGeometryCreatedEventArgs : System.EventArgs
{
    public StreamGeometry Geometry { get; }

    public TrackGeometryCreatedEventArgs(StreamGeometry geometry)
    {
        Geometry = geometry;
    }
}