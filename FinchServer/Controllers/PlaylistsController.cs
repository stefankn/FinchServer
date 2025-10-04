using FinchServer.Beets;
using FinchServer.Controllers.DTO;
using FinchServer.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinchServer.Controllers;

[ApiController]
[Route("/api/v1/playlists")]
public class PlaylistsController(
    DataContext dataContext,
    BeetsContext beetsContext
    ): ControllerBase {
    
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
    public async Task<ActionResult<PlaylistEntryDto>> CreateEntry(int id, [FromBody] CreatePlaylistEntryDto body) {
        var playlist = await dataContext.Playlists.FindAsync(id);
        if (playlist == null) return NotFound();

        var item = await beetsContext.Items.FindAsync(body.ItemId);
        if (item == null) return NotFound();
        
        await dataContext.Entry(playlist).Collection<PlaylistEntry>(p => p.Entries).LoadAsync();

        var currentIndex = playlist.Entries.Count > 0 ? playlist.Entries.Max(e => e.Index) : 0;
        var entry = new PlaylistEntry {
            Index = currentIndex + 1,
            ItemId = item.Id,
            CreatedAt = DateTime.Now
        };
        playlist.Entries.Add(entry);
        await dataContext.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = entry.Id }, new PlaylistEntryDto(entry, item));
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
    [Route("{id:int}")]
    public async Task<ActionResult> Delete(int id) {
        var playlist = await dataContext.Playlists.FindAsync(id);
        if (playlist == null) return NotFound();
        
        dataContext.Playlists.Remove(playlist);
        await dataContext.SaveChangesAsync();
        
        return NoContent();
    }
}