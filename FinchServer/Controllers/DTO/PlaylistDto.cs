using FinchServer.Database;

namespace FinchServer.Controllers.DTO;

public class PlaylistDto(Playlist playlist) {
    
    // - Properties
    
    public int Id { get; set; } = playlist.Id;
    public string Name { get; set; } = playlist.Name;
    public string? Description { get; set; } = playlist.Description;
    public bool IsArtworkAvailable { get; set; } = playlist.ImageFileName != null;
    public DateTimeOffset CreatedAt { get; set; } = playlist.CreatedAt.AddTicks( - (playlist.CreatedAt.Ticks % TimeSpan.TicksPerSecond));
    public DateTimeOffset UpdatedAt { get; set; } = playlist.UpdatedAt.AddTicks( - (playlist.UpdatedAt.Ticks % TimeSpan.TicksPerSecond));
}