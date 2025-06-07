using FinchServer.Controllers.DTO;
using FinchServer.Database;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinchServer.Controllers;

[ApiController]
[Route("/api/v1/albums")]
public class AlbumsController(
    IDbContextFactory<BeetsContext> dbContextFactory,
    IWebHostEnvironment environment
    ): ControllerBase {
    
    // - Functions

    [HttpGet]
    public async Task<ActionResult<Pager<AlbumDto>>> List(
        [FromQuery(Name = "filter")] string? filter,
        [FromQuery(Name = "sort")] string? sorting,
        [FromQuery(Name = "direction")] string? direction,
        [FromQuery(Name = "per")] int limit = 20,
        [FromQuery(Name = "page")] int page = 1
        ) {
        
        await using var dataContext = await dbContextFactory.CreateDbContextAsync();

        var query = dataContext.Albums.AsQueryable();
        if (!string.IsNullOrEmpty(filter)) {
            try {
                var albumFilter = Enum.Parse<AlbumFilter>(filter, true);
                var types = albumFilter.Types();
                var predicate = PredicateBuilder.New(query);
                foreach (var type in types) {
                    predicate = predicate.Or(a => a.AlbumType.Contains(type));
                }
                query = query.Where(predicate);
            } catch (Exception e) {
                return BadRequest("Invalid filter option");
            }
        }

        var albumSorting = Sorting.Added;
        var albumSortingDirection = SortingDirection.Ascending;

        if (!string.IsNullOrEmpty(sorting)) {
            try {
                albumSorting = Enum.Parse<Sorting>(sorting, true);
                if (!string.IsNullOrEmpty(direction)) {
                    albumSortingDirection = Enum.Parse<SortingDirection>(direction, true);
                }
            } catch (Exception e) {
                return BadRequest("Invalid sorting option");
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

        query = query.Skip((page - 1) * limit).Take(limit);

        var albums = await query.ToArrayAsync();
        var totalCount = await dataContext.Albums.CountAsync();
        
        return new Pager<AlbumDto>(albums.Select(a => new AlbumDto(a)).ToArray(), page, totalCount, limit);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AlbumDto>> Get(int id) {
        await using var dataContext = await dbContextFactory.CreateDbContextAsync();
        var album = await dataContext.Albums.FindAsync(id);
        
        return album == null ? NotFound() : new AlbumDto(album);
    }

    [HttpGet("{id:int}/items")]
    public async Task<ActionResult<ItemDto[]>> Items(int id) {
        await using var dataContext = await dbContextFactory.CreateDbContextAsync();
        var album = await dataContext.Albums.FindAsync(id);
        if (album == null) return NotFound();
        
        await dataContext.Entry(album).Collection(a => a.Items).LoadAsync();
        
        return album.Items.Select(i => new ItemDto(i)).ToArray();
    }

    [HttpGet("{id:int}/artwork")]
    public async Task<ActionResult> Artwork(int id) {
        await using var dataContext = await dbContextFactory.CreateDbContextAsync();
        var artworkPath = (await dataContext.Albums.FindAsync(id))?.ArtworkPath;
        if (artworkPath == null) return NotFound();
        
        var stream = new FileStream(artworkPath, FileMode.Open);
        return File(stream, "image/jpeg");
    }

    [HttpGet("{id:int}/artwork/thumbnail")]
    public async Task<ActionResult> ArtworkThumbnail(int id) {
        await using var dataContext = await dbContextFactory.CreateDbContextAsync();
        var album = await dataContext.Albums.FindAsync(id);
        if (album == null) return NotFound();

        var thumbnailPath = await album.ArtworkThumbnailPath(environment);
        if (thumbnailPath == null) return NotFound();
            
        var stream = new FileStream(thumbnailPath, FileMode.Open);
        return File(stream, "image/jpeg");
    }
}