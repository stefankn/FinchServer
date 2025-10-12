namespace FinchServer.Controllers.DTO;

public class ArtistDto {
    
    // - Properties
    
    public int? Id { get; set; }
    public string? MusicBrainzId { get; set; }
    public string? Name { get; set; }
    public string? Image { get; set; }
    public string? Thumbnail { get; set; }
    public string? BackgroundImage { get; set; }
    public string? Logo { get; set; }
}