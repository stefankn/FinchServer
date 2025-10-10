using Microsoft.EntityFrameworkCore;

namespace FinchServer.Database;

public class DataContext(DbContextOptions<DataContext> options): DbContext(options) {
    
    // - Properties
    
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<PlaylistEntry> PlaylistEntries { get; set; }
    public DbSet<Artist> Artists { get; set; }
    
    
    // - Functions
    
    // DbContext Functions

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<ArtistImage>()
            .Property(i => i.ImageType)
            .HasConversion<string>();
    }
}