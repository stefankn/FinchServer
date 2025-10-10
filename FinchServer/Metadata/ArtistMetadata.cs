using FinchServer.Database;

namespace FinchServer.Metadata;

public class ArtistMetadata {
    
    // - Types

    public class Artwork {
        public required string Url { get; init; }
        public ImageType Type { get; init; }
    }

    public class LocalArtwork {
        public required string FileName { get; init; }
        public ImageType Type { get; init; }
    }
    
    
    // - Properties
    
    public required int Id { get; init; }
    public required string MusicBrainzId { get; init; }
    public required string Name { get; init; }
    public required Artwork[] Artworks { get; init; }
    public List<LocalArtwork> LocalArtworks { get; set; } = [];
}