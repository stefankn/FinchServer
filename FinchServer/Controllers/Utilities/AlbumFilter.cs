namespace FinchServer.Controllers.Utilities;

public enum AlbumFilter {
    Album,
    Compilation,
    Single,
    Ep
}

public static class AlbumFilterExtensions {
    
    // - Functions

    public static string[] Types(this AlbumFilter filter) {
        return filter switch {
            AlbumFilter.Album => ["Album", "LP"],
            AlbumFilter.Compilation => ["Compilation", "Mixed"],
            AlbumFilter.Single => ["Single", "Maxi-Single"],
            AlbumFilter.Ep => ["EP"],
            _ => []
        };
    }
}