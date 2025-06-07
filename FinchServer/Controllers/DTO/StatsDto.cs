namespace FinchServer.Controllers.DTO;

public class StatsDto {
    
    // - Properties
    
    public int TrackCount { get; set; }
    public required string TotalTime { get; set; }
    public required string ApproximateTotalSize { get; set; }
    public int ArtistCount { get; set; }
    public int AlbumCount { get; set; }
    public int AlbumArtistCount { get; set; }
}