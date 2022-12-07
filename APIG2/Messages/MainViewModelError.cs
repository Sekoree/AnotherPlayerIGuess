namespace APIG2.Messages;

public class MainViewModelError
{
    public MainViewModelError(string message)
    {
        Message = message;
    }

    public string Message { get; }
}