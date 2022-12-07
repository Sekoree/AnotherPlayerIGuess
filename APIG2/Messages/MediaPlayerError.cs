using CommunityToolkit.Mvvm.Messaging.Messages;
using ManagedBass;

namespace APIG2.Messages;

public class MediaPlayerError
{
    public string Message { get; }
    public Errors? Exception { get; }

    public MediaPlayerError(string message, Errors? exception = null)
    {
        Message = message;
        Exception = exception;
    }
}