using FinchServer.Beets;
using FinchServer.Controllers.DTO;
using FinchServer.Database;
using FinchServer.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ILogger = FinchServer.Logging.ILogger;

namespace FinchServer.Controllers;

[ApiController]
[Route("/api/v1/playlists")]
public class PlaylistsController(
    DataContext dataContext,
    BeetsContext beetsContext,
    IWebHostEnvironment webHostEnvironment,
    ILogger logger
    ): ControllerBase {
    
    // - Private Properties
    
    private readonly string _playlistArtworkPath = Path.Combine(webHostEnvironment.ContentRootPath, "Resources", "Playlists");
    
    
    // - Functions

    [HttpGet]
    public async Task<ActionResult<PlaylistDto[]>> List() {
        return await dataContext.Playlists.AsNoTracking().Select(p => new PlaylistDto(p)).ToArrayAsync();
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<ActionResult<PlaylistDto>> Get(int id) {
        var playlist = await dataContext.Playlists.FindAsync(id);
        if (playlist == null) return NotFound();
        
        return new PlaylistDto(playlist);
    }

    [HttpGet]
    [Route("{id:int}/artwork")]
    public async Task<ActionResult> Image(int id) {
        var playlist = await dataContext.Playlists.FindAsync(id);
        if (playlist?.ImageFileName == null) return NotFound();
        
        var path = Path.Combine(_playlistArtworkPath, playlist.ImageFileName);
        var stream = new FileStream(path, FileMode.Open);
        return File(stream, "image/jpeg");
    }

    [HttpGet]
    [Route("{id:int}/entries")]
    public async Task<ActionResult<PlaylistEntryDto[]>> Entries(int id) {
        var playlist = await dataContext.Playlists.FindAsync(id);
        if (playlist == null) return NotFound();

        await dataContext.Entry(playlist).Collection<PlaylistEntry>(p => p.Entries).LoadAsync();
        var itemIds = playlist.Entries.Select(e => e.ItemId).ToArray();
        var items = await beetsContext.Items.AsNoTracking().Where(i => itemIds.Contains(i.Id)).ToArrayAsync();
        
        var entries = new List<PlaylistEntryDto>();
        foreach (var entry in playlist.Entries) {
            var item = items.FirstOrDefault(i => i.Id == entry.ItemId);
            if (item != null) {
                entries.Add(new PlaylistEntryDto(entry, item));
            }
        }

        return entries.ToArray();
    }

    [HttpPost]
    [Route("{id:int}/entries")]
    public async Task<ActionResult<PlaylistEntryDto[]>> CreateEntry(int id, [FromBody] CreatePlaylistEntryDto body) {
        var playlist = await dataContext.Playlists.FindAsync(id);
        if (playlist == null) return NotFound();

        var items = await beetsContext.Items.Where(i => body.ItemIds.Contains(i.Id)).ToArrayAsync();
        await dataContext.Entry(playlist).Collection<PlaylistEntry>(p => p.Entries).LoadAsync();
        
        var itemsOrdered = body.ItemIds
            .Select(itemId => items.FirstOrDefault(i => i.Id == itemId))
            .OfType<Item>()
            .ToArray();

        var currentIndex = playlist.Entries.Count > 0 ? playlist.Entries.Max(e => e.Index) : 0;
        var entries = itemsOrdered.Select((item, i) => new PlaylistEntry {
            Index = currentIndex + i + 1,
            ItemId = item.Id,
            CreatedAt = DateTime.Now
        }).ToArray();
        
        foreach (var entry in entries) {
            playlist.Entries.Add(entry);
        }
        
        await dataContext.SaveChangesAsync();
        
        return Ok(entries.Select(e => new PlaylistEntryDto(e, items.First(i => i.Id == e.ItemId))));
    }

    [HttpDelete]
    [Route("{id:int}/entries/{entryId:int}")]
    public async Task<ActionResult> DeleteEntry(int id, int entryId) {
        var playlist = await dataContext.Playlists.FindAsync(id);
        if (playlist == null) return NotFound();

        var entry = await dataContext.PlaylistEntries.FindAsync(entryId);
        if (entry == null) return NotFound();

        dataContext.PlaylistEntries.Remove(entry);
        await dataContext.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<PlaylistDto>> Create([FromBody] CreatePlaylistDto body) {
        var playlist = new Playlist {
            Name = body.Name,
            Description = body.Description,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Entries = new List<PlaylistEntry>()
        };
        
        if (body.Items != null) {
            var items = await beetsContext.Items.Where(i => body.Items.Contains(i.Id)).ToArrayAsync();
            
            foreach (var entry in items.Select((x, i) => new { Value = x, Index = i })) {
                playlist.Entries.Add(new PlaylistEntry {
                    Index = entry.Index,
                    ItemId = entry.Value.Id,
                    CreatedAt = DateTime.Now,
                });
            }
        }

        dataContext.Playlists.Add(playlist);
        await dataContext.SaveChangesAsync();
        
        return CreatedAtAction(nameof(Get), new { id = playlist.Id }, new PlaylistDto(playlist));
    }

    [HttpDelete]
    [Route("{id:int}/artwork")]
    public async Task<ActionResult<PlaylistDto>> DeleteArtwork(int id) {
        var playlist = await dataContext.Playlists.FindAsync(id);
        if (playlist == null) return NotFound();
        
        DeleteImageFile(playlist.ImageFileName);
        playlist.ImageFileName = null;
        await dataContext.SaveChangesAsync();

        return Ok(new PlaylistDto(playlist));
    }

    [HttpPut]
    [Route("{id:int}/artwork")]
    [RequestSizeLimit(1024 * 1024 * 1)]
    public async Task<ActionResult<PlaylistDto>> Image(int id, IFormFile image) {
        if (image.Length == 0) return BadRequest(new { message = "No image provided" });

        var allowedTypes = new[] { "image/jpeg", "image/png" };
        if (!allowedTypes.Contains(image.ContentType)) 
            return BadRequest(new { message = "Unsupported image format" });
        
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(image.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { error = "Invalid file extension" });
        
        var playlist = await dataContext.Playlists.FindAsync(id);
        if (playlist == null) return NotFound();
        
        // Delete existing image
        DeleteImageFile(playlist.ImageFileName);
        
        var filename = $"playlist_{id}-{Guid.NewGuid()}{extension}";
        var path = Path.Combine(_playlistArtworkPath, filename);
        
        await using var stream = new FileStream(path, FileMode.Create);
        await image.CopyToAsync(stream);

        playlist.ImageFileName = filename;
        await dataContext.SaveChangesAsync();
        
        return Ok(new PlaylistDto(playlist));
    }

    [HttpDelete]
    [Route("{id:int}")]
    public async Task<ActionResult> Delete(int id) {
        var playlist = await dataContext.Playlists.FindAsync(id);
        if (playlist == null) return NotFound();
        
        dataContext.Playlists.Remove(playlist);
        await dataContext.SaveChangesAsync();
        
        return NoContent();
    }
    
    
    // - Private Functions

    private void DeleteImageFile(string? filename) {
        if (filename == null) return;
        
        try {
            var existingImagePath = Path.Combine(_playlistArtworkPath, filename);
            if (System.IO.File.Exists(existingImagePath))
                System.IO.File.Delete(existingImagePath);
        } catch (Exception e) {
            logger.Error($"Unable to delete image file {filename}, {e.Message}", LogCategory.Web, consoleLog: true);
            logger.Error(e.ToString(), LogCategory.Web);
        }
    }
}