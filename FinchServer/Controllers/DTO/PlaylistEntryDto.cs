using FinchServer.Beets;
using FinchServer.Database;

namespace FinchServer.Controllers.DTO;

public class PlaylistEntryDto {
    
    // - Properties
    
    public int Id { get; set; }
    public int Index { get; set; }
    public ItemDto Item { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    
    
    // - Construction

    public PlaylistEntryDto(PlaylistEntry entry, Item item) {
        Id = entry.Id;
        Index = entry.Index;
        Item = new ItemDto(item);
        CreatedAt = entry.CreatedAt.AddTicks( - (entry.CreatedAt.Ticks % TimeSpan.TicksPerSecond));;
    }
}