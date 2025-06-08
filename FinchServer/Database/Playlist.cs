using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinchServer.Database;

[Table("playlists")]
public class Playlist {
    
    [Key]
    public int Id { get; set; }
    
    [Column("name")]
    public required string Name { get; set; }
    
    [Column("description")]
    public string? Description { get; set; }
    
    public required ICollection<PlaylistEntry> Entries { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}