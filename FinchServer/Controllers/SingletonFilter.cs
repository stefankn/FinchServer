namespace FinchServer.Controllers;

public enum SingletonFilter {
    Uncategorized,
    DjMix
}

public static class SingletonFilterExtensions {
    
    // - Functions

    public static string[] Types(this SingletonFilter filter) {
        return filter switch {
            SingletonFilter.DjMix => ["dj-mix"],
            _ => []
        };
    }
}