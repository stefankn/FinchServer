namespace FinchServer.Metadata;

public interface IMetadataFetcher {
    
    // - Functions

    public Task<ArtistMetadata> FetchArtistArtwork(int discogsArtistId);
}