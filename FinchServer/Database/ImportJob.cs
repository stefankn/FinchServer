using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinchServer.Database;

[Table("import_jobs")]
public class ImportJob {
    
    // - Properties
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(36)]
    public required string Id { get; init; }
    
    [MaxLength(255)]
    public required string Path { get; init; }
    
    public required ICollection<ImportFile> Files { get; init; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; init; }
}