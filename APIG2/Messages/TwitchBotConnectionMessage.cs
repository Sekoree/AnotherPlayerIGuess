namespace APIG2.Messages;

public class TwitchBotConnectionMessage
{
    public TwitchBotConnectionStatus Status { get; }
    public string Message { get; }

    public TwitchBotConnectionMessage(TwitchBotConnectionStatus status, string message)
    {
        Status = status;
        Message = message;
    }
}

public enum TwitchBotConnectionStatus
{
    Connected,
    Disconnected,
    Error
}