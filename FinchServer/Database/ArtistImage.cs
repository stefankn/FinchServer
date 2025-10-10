using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinchServer.Database;

[Table("artist_image")]
public class ArtistImage {
    
    // - Properties
    
    [Key]
    public int Id { get; init; }
    
    [MaxLength(255)] 
    public required string FileName { get; init; }
    
    public ImageType ImageType { get; init; }
    public Artist Artist { get; set; } = null!;
}