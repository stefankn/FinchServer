using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace FinchServer.Beets;

[Table("albums")]
public class Album {
    
    // - Properties
    
    [Key]
    public int Id { get; set; }
    
    [Column("album")]
    public required string Title { get; set; }
    
    [Column("albumtype")]
    public required string AlbumType { get; set; }
    
    [Column("albumartist")]
    public required string AlbumArtist { get; set; }
    
    [Column("added")]
    public required double Added { get; set; }
    
    [Column("genre")]
    public string? Genre { get; set; }
    
    [Column("style")]
    public string? Style { get; set; }
    
    [Column("year")]
    public int Year { get; set; }
    
    [Column("disctotal")]
    public int DiscCount { get; set; }
    
    [Column("discogs_albumid")]
    public int? DiscogsAlbumId { get; set; }
    
    [Column("discogs_artistid")]
    public int? DiscogsArtistId { get; set; }
    
    [Column("discogs_labelid")]
    public int? DiscogsLabelId { get; set; }
    
    [Column("label")]
    public string? Label { get; set; }
    
    [Column("barcode")]
    public string? Barcode { get; set; }
    
    [Column("asin")]
    public string? Asin { get; set; }
    
    [Column("catalognum")]
    public string? CatalogNumber { get; set; }
    
    [Column("country")]
    public string? Country { get; set; }
    
    [Column("artpath")]
    public byte[]? ArtworkPathData { get; set; }
    
    public required ICollection<Item> Items { get; set; }

    public string? ArtworkPath => ArtworkPathData == null ? null : Encoding.UTF8.GetString(ArtworkPathData);
    public bool IsArtworkAvailable => ArtworkPathData != null && ArtworkPathData.Length > 0;

    public string? ArtworkThumbnailFilename {
        get {
            if (DiscogsAlbumId == null || ArtworkPath == null) return null;
            var extension = Path.GetExtension(ArtworkPath);
            
            return $"album_{DiscogsAlbumId}_thumbnail{extension}";
        }
    }
    
    
    // - Functions

    public async Task<string?> ArtworkThumbnailPath(IWebHostEnvironment environment) {
        if (ArtworkThumbnailFilename == null) return null;
        
        var thumbnailPath = Path.Combine(environment.ContentRootPath, "Resources/Thumbnails", ArtworkThumbnailFilename);
        if (File.Exists(thumbnailPath)) return thumbnailPath;
        
        if (ArtworkPath == null) return null;
        
        using var image = await Image.LoadAsync(ArtworkPath);
        image.Mutate(i => i.Resize(300, 0));
        await image.SaveAsync(thumbnailPath);

        return thumbnailPath;
    }
}