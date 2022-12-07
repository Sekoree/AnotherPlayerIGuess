using System;
using ManagedBass;

namespace APIG2.Messages;

public class TrackPrepareError
{
    public string Message { get; }
    public Exception? Exception { get; }
    public Errors? BassError { get; }

    public TrackPrepareError(string message, Exception? exception = null, Errors? bassError = null)
    {
        Message = message;
        Exception = exception;
        BassError = bassError;
    }
}