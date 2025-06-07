using FinchServer.Beets;
using Microsoft.EntityFrameworkCore;

namespace FinchServer.Database;

public class BeetsContext(BeetsConfiguration configuration): DbContext {
    
    // - Properties
    
    public DbSet<Album> Albums { get; set; }
    public DbSet<Item> Items { get; set; }
    
    
    // - Functions
    
    // DbContext Functions

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseSqlite($"Data Source={configuration.DatabasePath}");
    }
}