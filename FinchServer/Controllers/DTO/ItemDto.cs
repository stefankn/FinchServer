using FinchServer.Beets;

namespace FinchServer.Controllers.DTO;

public class ItemDto(Item item) {
    
    // - Properties
    
    public int Id { get; set; } = item.Id;
    public int? Track { get; set; } = item.Track;
    public int? Disc { get; set; } = item.Disc;
    public string Title { get; set; } = item.Title;
    public string Artist { get; set; } = item.Artist;
    public double Length { get; set; } = item.Length;
    public string Format { get; set; } = item.Format;
    public int Bitrate { get; set; } = item.Bitrate;
    public int SampleRate { get; set; } = item.SampleRate;
    public string? Genre { get; set; } = !string.IsNullOrEmpty(item.Genre) ? item.Genre : null;
    public string? Style { get; set; } = !string.IsNullOrEmpty(item.Style) ? item.Style : null;
    public string? AlbumType { get; set; } = item.AlbumType;
}