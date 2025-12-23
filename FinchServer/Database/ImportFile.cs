using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinchServer.Database;

[Table("import_files")]
public class ImportFile {
    
    // - Properties
    
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [MaxLength(255)]
    public required string Filename { get; set; }
    
    [MaxLength(255)]
    public string? Path { get; set; }
    
    [MaxLength(255)]
    public string? ErrorMessage { get; set; }
    
    public ImportJob ImportJob { get; set; } = null!;
    
    public bool IsSuccess => ErrorMessage == null;
}