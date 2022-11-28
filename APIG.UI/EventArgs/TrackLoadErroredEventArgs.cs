using System;
using APIG.UI.Models;

namespace APIG.UI.EventArgs;

public class TrackLoadErroredEventArgs : System.EventArgs
{
    public IBaseTrack? Track { get; }
    public Exception Exception { get; }

    public TrackLoadErroredEventArgs(IBaseTrack? track, Exception exception)
    {
        Track = track;
        Exception = exception;
    }
}