using FinchServer.Beets;
using FinchServer.Metadata;
using FinchServer.Controllers.DTO;
using FinchServer.Database;
using Microsoft.AspNetCore.Mvc;

namespace FinchServer.Controllers;

[ApiController]
[Route("/api/v1/artists")]
public class ArtistsController(
    DataContext dataContext,
    IMetadataManager metadataManager
): ControllerBase {
    
    // - Functions

    [HttpGet("{discogsArtistId:int}")]
    public async Task<ActionResult<ArtistDto>> Get(int discogsArtistId) {
        var artist = await dataContext.FindAsync<Artist>(discogsArtistId);
        if (artist == null) {
            var artistMetadata = await metadataManager.FetchArtistMetaData(discogsArtistId);
            if (artistMetadata == null) return NotFound();

            artist = new Artist {
                Id = artistMetadata.Id,
                MusicBrainzId = artistMetadata.MusicBrainzId,
                Name = artistMetadata.Name,
                Images = new List<ArtistImage>(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            
            foreach (var artwork in artistMetadata.LocalArtworks) {
                artist.Images.Add(new ArtistImage { FileName = artwork.FileName, ImageType = artwork.Type });
            }
            
            dataContext.Artists.Add(artist);
            await dataContext.SaveChangesAsync();
        }
        
        return new ArtistDto {
            Id = artist.Id,
            MusicBrainzId = artist.MusicBrainzId,
            Name = artist.Name,
        };
    }
}