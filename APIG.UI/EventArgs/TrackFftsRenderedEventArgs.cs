namespace APIG.UI.EventArgs;

public class TrackFftsRenderedEventArgs : System.EventArgs
{
    public float[] Ffts { get; }
    public float[] MaxFft { get; }

    public TrackFftsRenderedEventArgs(float[] ffts, float[] maxFft)
    {
        Ffts = ffts;
        MaxFft = maxFft;
    }
}