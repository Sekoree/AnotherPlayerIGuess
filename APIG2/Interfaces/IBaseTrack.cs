using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace APIG2.Interfaces;

public interface IBaseTrack
{
    #region Base Info
    
    public string Title { get; set; }
    public string Artist { get; set; }
    public TimeSpan Duration { get; set; }
    public string AlbumArtUri { get; set; }
    public Uri? Source { get; set; }

    public bool InfoLoaded { get; set; }
    public Task<bool> LoadInfoAsync();

    #endregion
    
    #region Data
    
    public double StreamPercentLoaded { get; set; }
    public PreparationStatus PrepStatus { get; set; }
    public float[] FftMaxPerSegment { get; set; }
    
    public double Loudness { get; set; }
    
    public byte[]? AudioStream { get; set; }
    
    public Task<bool> LoadAudioStreamAsync();
    public Task<bool> LoadFftDataAsync();
    public Task<bool> LoadLoudnessAsync();
    public Task<bool> PrepareTrackAsync();
    public Task UnprepareTrackAsync();

    #endregion
}

public enum PreparationStatus
{
    NotPrepared,
    Preparing,
    Prepared,
    Failed
}