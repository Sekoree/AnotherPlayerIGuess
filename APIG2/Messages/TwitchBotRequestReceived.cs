namespace APIG2.Messages;

public class TwitchBotRequestReceived
{
    public string VideoUrl { get; }

    public TwitchBotRequestReceived(string videoUrl)
    {
        VideoUrl = videoUrl;
    }
}