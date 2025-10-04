using FinchServer.Beets;
using FinchServer.Controllers.DTO;
using FinchServer.Controllers.Utilities;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinchServer.Controllers;

[ApiController]
[Route("/api/v1/items")]
public class ItemsController(BeetsContext beetsContext): ControllerBase {

    // - Functions

    [HttpGet]
    public async Task<ActionResult<Pager<ItemDto>>> List(
        [FromQuery(Name = "filter")] string? filter,
        [FromQuery(Name = "sort")] string? sorting,
        [FromQuery(Name = "direction")] string? direction,
        [FromQuery(Name = "per")] int limit = 20,
        [FromQuery(Name = "page")] int page = 1
        ) {

        SingletonFilter? singletonFilter = null;
        if (!string.IsNullOrEmpty(filter)) {
            if (!Enum.TryParse<SingletonFilter>(filter.Replace("-", ""), true, out var parsedSingletonFilter)) 
                return BadRequest("Invalid filter option");
            
            singletonFilter = parsedSingletonFilter;
        }
        
        var singletonSorting = Sorting.Added;
        var singletonSortingDirection = SortingDirection.Ascending;
        if (!string.IsNullOrEmpty(sorting)) {
            if (!Enum.TryParse(sorting, true, out singletonSorting))
                return BadRequest("Invalid sorting option");
        }
        
        if (!string.IsNullOrEmpty(direction)) {
            if (!Enum.TryParse(direction, true, out singletonSortingDirection))
                return BadRequest("Invalid sorting direction");
        }
        
        var query = beetsContext.Items.AsNoTracking().Where(i => i.AlbumId == null);

        if (singletonFilter.HasValue) {
            if (singletonFilter == SingletonFilter.Uncategorized) {
                query = query.Where(i => i.AlbumType == "");
            } else {
                var types = singletonFilter.Value.Types();
                var predicate = PredicateBuilder.New(query);
                foreach (var type in types) {
                    predicate = predicate.Or(i => i.AlbumType.Contains(type));
                }
                query = query.Where(predicate);
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
        
        var skip = (page - 1) * limit;
        var countTask = query.CountAsync();
        var dataTask = query.Skip(skip).Take(limit).Select(a => new ItemDto(a)).ToArrayAsync();
        await Task.WhenAll(countTask, dataTask);
        
        return new Pager<ItemDto>(await dataTask, page, await countTask, limit);
    }

    [HttpGet("{id:int}/stream")]
    public async Task<ActionResult> Stream(int id) {
        var path = (await beetsContext.Items.FindAsync(id))?.Path;
        if (path == null || !System.IO.File.Exists(path)) return NotFound();
        
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return new FileStreamResult(stream, "audio/basic") {
            EnableRangeProcessing = true
        };
    }
}