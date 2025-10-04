using Microsoft.EntityFrameworkCore;

namespace FinchServer.Beets;

public class BeetsContext(DbContextOptions<BeetsContext> options): DbContext(options) {
    
    // - Properties
    
    public DbSet<Album> Albums { get; set; }
    public DbSet<Item> Items { get; set; }
}