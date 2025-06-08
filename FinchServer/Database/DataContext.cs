using Microsoft.EntityFrameworkCore;

namespace FinchServer.Database;

public class DataContext(IWebHostEnvironment environment): DbContext {
    
    // - Properties
    
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<PlaylistEntry> PlaylistEntries { get; set; }

    
    // - Functions

    public async Task Migrate() {
        if ((await Database.GetPendingMigrationsAsync()).Any()) {
            await Database.MigrateAsync();
        }
    }
    
    // DbContext Functions

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        var path = Path.Combine(environment.ContentRootPath, "data", "Finch.db");
        optionsBuilder.UseSqlite($"Data Source={path}");
    }
}