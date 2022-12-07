using System;

namespace APIG2.Messages;

public class TrackError
{
    public string Message { get; }
    public Exception? Exception { get; }

    public TrackError(string message, Exception? exception = null)
    {
        Message = message;
        Exception = exception;
    }
}