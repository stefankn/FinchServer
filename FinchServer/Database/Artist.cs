using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinchServer.Database;

[Table("artists")]
public class Artist {
    
    // - Properties
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required int Id { get; init; }
    
    [MaxLength(255)] 
    public required string Name { get; init; }
    
    [MaxLength(255)]
    public required string MusicBrainzId { get; init; }
    
    public required ICollection<ArtistImage> Images { get; set; }
    
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }
}