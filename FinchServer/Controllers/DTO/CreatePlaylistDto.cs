namespace FinchServer.Controllers.DTO;

public class CreatePlaylistDto {
    
    // - Properties
    
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int[]? Items { get; set; }
}