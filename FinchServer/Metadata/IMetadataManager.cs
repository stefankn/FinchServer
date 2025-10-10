namespace FinchServer.Metadata;

public interface IMetadataManager {
    
    // - Properties
    
    public string ArtistArtworkPath { get; }
    
    
    // - Functions

    public Task<ArtistMetadata?> FetchArtistMetaData(int discogsArtistId);
}