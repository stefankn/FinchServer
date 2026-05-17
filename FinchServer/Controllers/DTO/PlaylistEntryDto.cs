using FinchServer.Beets;
using FinchServer.Database;

namespace FinchServer.Controllers.DTO;

public class PlaylistEntryDto(PlaylistEntry entry, Item item, Playlist playlist) {
    
    // - Properties
    
    public int Id { get; set; } = entry.Id;
    public int Index { get; set; } = entry.Index;
    public ItemDto Item { get; set; } = new(item);
    public string PlaylistName { get; set; } = playlist.Name;
    public DateTimeOffset CreatedAt { get; set; } = entry.CreatedAt.AddTicks( - (entry.CreatedAt.Ticks % TimeSpan.TicksPerSecond));
}