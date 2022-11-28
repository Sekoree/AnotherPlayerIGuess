using ManagedBass;

namespace APIG.UI.EventArgs;

public class PlayerErroredEventArgs : System.EventArgs
{
    public Errors? Error { get; }
    public string Description { get; }


    public PlayerErroredEventArgs(Errors? error, string description)
    {
        Error = error;
        Description = description;
    }
}