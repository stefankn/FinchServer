using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinchServer.Beets;

namespace FinchServer.Database;

[Table("playlist_entries")]
public class PlaylistEntry {
    
    // - Properties
    
    [Key]
    public int Id { get; set; }
    
    [Column("index")]
    public required int Index { get; set; }
    
    [Column("item_id")]
    public required int ItemId { get; set; }

    public DateTime CreatedAt { get; set; }
    
    public Playlist Playlist { get; set; } = null!;
}