using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FinchServer.Beets;

[Table("items")]
public class Item {
    
    // - Properties
    
    [Key]
    public int Id { get; set; }
    
    [Column("track")]
    public int? Track { get; set; }
    
    [Column("disc")]
    public int? Disc { get; set; }
    
    [Column("title")]
    public required string Title { get; set; }
    
    [Column("artist")]
    public required string Artist { get; set; }
    
    [Column("path")]
    public required byte[] PathData { get; set; }
    
    [Column("length")]
    public double Length { get; set; }
    
    [Column("format")]
    public required string Format { get; set; }
    
    [Column("bitrate")]
    public int Bitrate { get; set; }
    
    [Column("samplerate")]
    public int SampleRate { get; set; }
    
    [Column("album_id")]
    public int? AlbumId { get; set; }
    
    [Column("albumtype")]
    public required string AlbumType { get; set; }
    
    [Column("genre")]
    public string? Genre { get; set; }
    
    [Column("style")]
    public string? Style { get; set; }
    
    [Column("added")]
    public required double Added { get; set; }
    
    [Column("discogs_artistid")]
    public int? DiscogsArtistId { get; set; }
    
    public Album? Album { get; set; }
    
    public string Path => Encoding.UTF8.GetString(PathData);
    public TimeSpan Duration => TimeSpan.FromSeconds(Length);

    public string DurationDescription {
        get {
            var duration = Duration;
            return duration.Hours > 0
                ? $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}"
                : $"{duration.Minutes:D2}:{duration.Seconds:D2}";
        }
    }
}