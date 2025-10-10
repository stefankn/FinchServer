namespace FinchServer.Metadata;

public interface IMetadataManager {
    
    // - Functions

    public Task<ArtistMetadata?> FetchArtistMetaData(int discogsArtistId);
}