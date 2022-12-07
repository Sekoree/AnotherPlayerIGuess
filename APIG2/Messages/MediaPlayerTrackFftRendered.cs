namespace APIG2.Messages;

public class MediaPlayerTrackFftRendered
{
    public float[] FftData { get; }

    public MediaPlayerTrackFftRendered(float[] fftData)
    {
        FftData = fftData;
    }
}