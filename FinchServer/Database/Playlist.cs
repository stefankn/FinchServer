using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinchServer.Database;

[Table("playlists")]
public class Playlist {
    
    [Key]
    public int Id { get; init; }
    
    [MaxLength(255)] 
    public required string Name { get; set; }
    
    [Column(TypeName = "text")]
    public string? Description { get; set; }
    
    public required ICollection<PlaylistEntry> Entries { get; set; }
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime UpdatedAt { get; set; }
}