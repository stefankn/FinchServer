using FinchServer.Beets;
using FinchServer.Controllers.DTO;
using FinchServer.Controllers.Utilities;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinchServer.Controllers;

[ApiController]
[Route("/api/v1/items")]
public class ItemsController(IDbContextFactory<BeetsContext> dbContextFactory): ControllerBase {
    
    // - Functions

    [HttpGet]
    public async Task<ActionResult<Pager<ItemDto>>> List(
        [FromQuery(Name = "filter")] string? filter,
        [FromQuery(Name = "sort")] string? sorting,
        [FromQuery(Name = "direction")] string? direction,
        [FromQuery(Name = "per")] int limit = 20,
        [FromQuery(Name = "page")] int page = 1
        ) {
        await using var dataContext = await dbContextFactory.CreateDbContextAsync();
        var query = dataContext.Items.Where(i => i.AlbumId == null);

        if (!string.IsNullOrEmpty(filter)) {
            try {
                // TODO: map dj-mix filter
                var singletonFilter = Enum.Parse<SingletonFilter>(filter, true);
                if (singletonFilter == SingletonFilter.Uncategorized) {
                    query = query.Where(i => i.AlbumType == "");
                } else {
                    var types = singletonFilter.Types();
                    var predicate = PredicateBuilder.New(query);
                    foreach (var type in types) {
                        predicate = predicate.Or(i => i.AlbumType.Contains(type));
                    }
                    query = query.Where(predicate);
                }
            } catch (Exception e) {
                return BadRequest("Invalid filter option");
            }
        }
        
        var singletonSorting = Sorting.Added;
        var singletonSortingDirection = SortingDirection.Ascending;

        if (!string.IsNullOrEmpty(sorting)) {
            try {
                singletonSorting = Enum.Parse<Sorting>(sorting, true);
                if (!string.IsNullOrEmpty(direction)) {
                    singletonSortingDirection = Enum.Parse<SortingDirection>(direction, true);
                }
            } catch (Exception e) {
                return BadRequest("Invalid sorting option");
            }
        }
        
        query = singletonSorting switch {
            Sorting.Added => singletonSortingDirection == SortingDirection.Ascending
                ? query.OrderBy(a => a.Added)
                : query.OrderByDescending(a => a.Added),
            Sorting.Title => singletonSortingDirection == SortingDirection.Ascending
                ? query.OrderBy(a => a.Title)
                : query.OrderByDescending(a => a.Title),
            Sorting.Artist => singletonSortingDirection == SortingDirection.Ascending
                ? query.OrderBy(a => a.Artist)
                : query.OrderByDescending(a => a.Artist),
            _ => query
        };
        
        query = query.Skip((page - 1) * limit).Take(limit);

        var items = await query.ToArrayAsync();
        var totalCount = await dataContext.Items.Where(i => i.AlbumId == null).CountAsync();
        
        return new Pager<ItemDto>(items.Select(i => new ItemDto(i)).ToArray(), page, totalCount, limit);
    }

    [HttpGet("{id:int}/stream")]
    public async Task<ActionResult> Stream(int id) {
        await using var dataContext = await dbContextFactory.CreateDbContextAsync();
        var path = (await dataContext.Items.FindAsync(id))?.Path;
        if (path == null || !System.IO.File.Exists(path)) return NotFound();
        
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return new FileStreamResult(stream, "audio/basic") {
            EnableRangeProcessing = true
        };
    }
}