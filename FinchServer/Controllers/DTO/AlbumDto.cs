using FinchServer.Beets;

namespace FinchServer.Controllers.DTO;

public class AlbumDto(Album album) {
    
    // - Properties
    
    public int Id { get; set; } = album.Id;
    public string Title { get; set; } = album.Title;
    public string Type { get; set; } = album.AlbumType;
    public string Artist { get; set; } = album.AlbumArtist;
    public string? Genre { get; set; } = album.Genre;
    public string? Style { get; set; } = album.Style;
    public string? Country { get; set; } = album.Country;
    public int Year { get; set; } = album.Year;
    public int DiscCount { get; set; } = album.DiscCount;
    public int? DiscogsAlbumId { get; set; } = (album.DiscogsAlbumId ?? 0) > 0 ? album.DiscogsAlbumId : null;
    public int? DiscogsArtistId { get; set; } = (album.DiscogsArtistId ?? 0) > 0 ? album.DiscogsArtistId : null;
    public int? DiscogsLabelId { get; set; } = (album.DiscogsLabelId ?? 0) > 0 ? album.DiscogsLabelId : null;
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.FromUnixTimeSeconds((long)album.Added);
    public string? Label { get; set; } = album.Label;
    public string? Barcode { get; set; } = !string.IsNullOrEmpty(album.Barcode) ? album.Barcode : null;
    public string? Asin { get; set; } = !string.IsNullOrEmpty(album.Asin) ? album.Asin : null;
    public string? CatalogNumber { get; set; } = !string.IsNullOrEmpty(album.CatalogNumber) ? album.CatalogNumber : null;
    public string? ArtworkPath { get; set; } = album.ArtworkPath;
    public bool IsArtworkAvailable { get; set; } = album.IsArtworkAvailable;
    public ItemDto[]? Items { get; set; } = album.Items?.Select(i => new ItemDto(i)).ToArray();
}