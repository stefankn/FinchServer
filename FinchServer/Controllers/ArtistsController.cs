using FinchServer.Metadata;
using FinchServer.Controllers.DTO;
using FinchServer.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        var artist = await GetArtist(discogsArtistId);
        if (artist == null) return NotFound();
        
        return new ArtistDto {
            Id = artist.Id,
            Name = artist.Name,
            MusicBrainzId = artist.MusicBrainzId,
            Image = artist.Images.FirstOrDefault(i => i.ImageType == ImageType.Image)?.FileName,
            Thumbnail = artist.Images.FirstOrDefault(i => i.ImageType == ImageType.Thumbnail)?.FileName,
            BackgroundImage = artist.Images.FirstOrDefault(i => i.ImageType == ImageType.Background)?.FileName,
            Logo = artist.Images.FirstOrDefault(i => i.ImageType == ImageType.Logo)?.FileName
        };
    }

    [HttpGet("{discogsArtistId:int}/background")]
    public async Task<ActionResult> Background(int discogsArtistId) {
        return await ServeArtwork(discogsArtistId, ImageType.Background);
    }
    
    [HttpGet("{discogsArtistId:int}/image")]
    public async Task<ActionResult> Image(int discogsArtistId) {
        return await ServeArtwork(discogsArtistId, ImageType.Image);
    }
    
    [HttpGet("{discogsArtistId:int}/thumbnail")]
    public async Task<ActionResult> Thumbnail(int discogsArtistId) {
        return await ServeArtwork(discogsArtistId, ImageType.Thumbnail);
    }
    
    [HttpGet("{discogsArtistId:int}/logo")]
    public async Task<ActionResult> Logo(int discogsArtistId) {
        return await ServeArtwork(discogsArtistId, ImageType.Logo);
    }
    
    
    // - Private Functions
    
    private async Task<ActionResult> ServeArtwork(int discogsArtistId, ImageType imageType) {
        var artist = await GetArtist(discogsArtistId);
        var image = artist?.Images.FirstOrDefault(i => i.ImageType == imageType);
        if (artist == null || image == null) return NotFound();
        
        var path = Path.Combine(metadataManager.ArtistArtworkPath, artist.Id.ToString(), image.FileName);
        var stream = new FileStream(path, FileMode.Open);
        return File(stream, "image/jpeg");
    }

    private async Task<Artist?> GetArtist(int discogsArtistId) {
        var artist = await dataContext.Artists
            .Include(a => a.Images)
            .FirstOrDefaultAsync(a => a.Id == discogsArtistId);
        
        if (artist == null) {
            var artistMetadata = await metadataManager.FetchArtistMetaData(discogsArtistId);
            if (artistMetadata == null) return null;

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

        return artist;
    }
}