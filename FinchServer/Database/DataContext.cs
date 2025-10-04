using Microsoft.EntityFrameworkCore;

namespace FinchServer.Database;

public class DataContext(DbContextOptions<DataContext> options): DbContext(options) {
    
    // - Properties
    
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<PlaylistEntry> PlaylistEntries { get; set; }
}