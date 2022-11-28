using System;
using System.Threading.Tasks;

namespace APIG.UI.Models;

public interface IBaseTrack
{
    public string Title { get; set; }
    public string Artist { get; set; }
    public TimeSpan Duration { get; set; }
    public string AlbumArtUri { get; set; }
    public Uri Source { get; set; }
    public bool InfoLoaded { get; set; }
    
    public Task<bool> LoadInfoAsync(bool skipArt = false);
    
    public Task<Uri?> GetPathAsync();
}