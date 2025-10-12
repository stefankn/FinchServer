using FinchServer.Database;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace FinchServer.Metadata;

public class MetadataManager(
    IMetadataFetcher metadataFetcher,
    IWebHostEnvironment webHostEnvironment,
    IHttpClientFactory httpClientFactory
): IMetadataManager {
    
    // - Properties
    
    // IMetadataManager Properties
    
    public string ArtistArtworkPath => Path.Combine(webHostEnvironment.ContentRootPath, "Resources", "Artists");
    
    
    // - Functions
    
    // IMetadataManager Functions

    public async Task<ArtistMetadata?> FetchArtistMetaData(int discogsArtistId) {
        try {
            var artistMetadata = await metadataFetcher.FetchArtistArtwork(discogsArtistId);
            
            var tasks = artistMetadata.Artworks.Select(a => Download(a, artistMetadata));
            await Task.WhenAll(tasks);
        
            return artistMetadata;
        } catch (Exception e) {
            Console.WriteLine($"Failed to fetch artist metadata, {e}");
            return null;
        }
    }
    
    
    // - Private Functions

    private async Task Download(ArtistMetadata.Artwork artwork, ArtistMetadata metadata) {
        try {
            var directory = Path.Combine(ArtistArtworkPath, metadata.Id.ToString());
            Directory.CreateDirectory(directory);

            var uri = new Uri(artwork.Url);
            var filename = Path.GetFileName(uri.LocalPath);
            var path = Path.Combine(directory, filename);

            if (!File.Exists(path)) {
                var client = httpClientFactory.CreateClient();
                await using var stream = await client.GetStreamAsync(uri);
                await using (var fileStream = File.Create(path)) {
                    await stream.CopyToAsync(fileStream);
                }
                
                metadata.LocalArtworks.Add(new ArtistMetadata.LocalArtwork {
                    FileName = filename,
                    Type = artwork.Type,
                });

                // Create thumbnail for regular images
                if (artwork.Type == ImageType.Image) {
                    var thumbnailFilename = $"thumb_{filename}";
                    var thumbnailPath = Path.Combine(directory, thumbnailFilename);
                    
                    using var image = await Image.LoadAsync(path);
                    image.Mutate(i => i.Resize(300, 0));
                    await image.SaveAsync(thumbnailPath);
                    
                    metadata.LocalArtworks.Add(new ArtistMetadata.LocalArtwork {
                        FileName = thumbnailFilename,
                        Type = ImageType.Thumbnail,
                    });
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex);
        }
    }
}