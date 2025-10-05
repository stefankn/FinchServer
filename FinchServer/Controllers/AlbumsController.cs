using FinchServer.Beets;
using FinchServer.Controllers.DTO;
using FinchServer.Controllers.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinchServer.Controllers;

[ApiController]
[Route("/api/v1/albums")]
public class AlbumsController(
    BeetsContext beetsContext,
    IWebHostEnvironment environment
    ): ControllerBase {
    
    // - Functions

    [HttpGet]
    public async Task<ActionResult<Pager<AlbumDto>>> List(
        [FromQuery(Name = "filter")] string? filter,
        [FromQuery(Name = "search")] string? search,
        [FromQuery(Name = "sort")] string? sorting,
        [FromQuery(Name = "direction")] string? direction,
        [FromQuery(Name = "per")] int limit = 20,
        [FromQuery(Name = "page")] int page = 1
        ) {

        AlbumFilter? albumFilter = null;
        if (!string.IsNullOrEmpty(filter)) {
            if (!Enum.TryParse<AlbumFilter>(filter, true, out var parsedAlbumFilter)) 
                return BadRequest("Invalid filter option");
            
            albumFilter = parsedAlbumFilter;
        }
        
        var albumSorting = Sorting.Added;
        var albumSortingDirection = SortingDirection.Ascending;
        if (!string.IsNullOrEmpty(sorting)) {
            if (!Enum.TryParse(sorting, true, out albumSorting))
                return BadRequest("Invalid sorting option");
        }

        if (!string.IsNullOrEmpty(direction)) {
            if (!Enum.TryParse(direction, true, out albumSortingDirection))
                return BadRequest("Invalid sorting direction");
        }
        
        var query = beetsContext.Albums.AsNoTracking();

        if (albumFilter.HasValue) {
            var types = albumFilter.Value.Types();
            query = query.Where(a => types.Any(type => a.AlbumType.Contains(type)));
        }

        search = search?.Trim();
        if (!string.IsNullOrEmpty(search)) {
            var terms = search.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            if (terms.Length > 0) {
                foreach (var term in terms) {
                    query = query.Where(a =>
                        EF.Functions.Like(a.AlbumArtist, $"%{term}%") ||
                        EF.Functions.Like(a.Title, $"%{term}%")
                    );
                }
            }
        }
        
        query = albumSorting switch {
            Sorting.Added => albumSortingDirection == SortingDirection.Ascending
                ? query.OrderBy(a => a.Added)
                : query.OrderByDescending(a => a.Added),
            Sorting.Title => albumSortingDirection == SortingDirection.Ascending
                ? query.OrderBy(a => a.Title)
                : query.OrderByDescending(a => a.Title),
            Sorting.Artist => albumSortingDirection == SortingDirection.Ascending
                ? query.OrderBy(a => a.AlbumArtist)
                : query.OrderByDescending(a => a.AlbumArtist),
            _ => query
        };
        
        var skip = (page - 1) * limit;
        var countTask = query.CountAsync();
        var dataTask = query.Skip(skip).Take(limit).Select(a => new AlbumDto(a)).ToArrayAsync();
        await Task.WhenAll(countTask, dataTask);
    
        return new Pager<AlbumDto>(await dataTask, page, await countTask, limit);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AlbumDto>> Get(int id) {
        var album = await beetsContext.Albums.FindAsync(id);
        
        return album == null ? NotFound() : new AlbumDto(album);
    }

    [HttpGet("{id:int}/items")]
    public async Task<ActionResult<ItemDto[]>> Items(int id) {
        var album = await beetsContext.Albums.FindAsync(id);
        if (album == null) return NotFound();
        
        await beetsContext.Entry(album).Collection(a => a.Items).LoadAsync();
        
        return album.Items.Select(i => new ItemDto(i)).ToArray();
    }

    [HttpGet("{id:int}/artwork")]
    public async Task<ActionResult> Artwork(int id) {
        var artworkPath = (await beetsContext.Albums.FindAsync(id))?.ArtworkPath;
        if (artworkPath == null) return NotFound();
        
        var stream = new FileStream(artworkPath, FileMode.Open);
        return File(stream, "image/jpeg");
    }

    [HttpGet("{id:int}/artwork/thumbnail")]
    public async Task<ActionResult> ArtworkThumbnail(int id) {
        var album = await beetsContext.Albums.FindAsync(id);
        if (album == null) return NotFound();

        var thumbnailPath = await album.ArtworkThumbnailPath(environment);
        if (thumbnailPath == null) return NotFound();
            
        var stream = new FileStream(thumbnailPath, FileMode.Open);
        return File(stream, "image/jpeg");
    }
}