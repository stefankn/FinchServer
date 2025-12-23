namespace FinchServer.Components.Pages.Import;

public class ImportConfiguration {
    
    // - Types

    public enum ImportType {
        Album,
        SingleTrack,
        DjMix
    }
    
    
    // - Properties
    
    public ImportType Type { get; set; }
    public bool KeepExistingMetadata { get; set; }
    public bool ImportAsSingleton { get; set; }
    public string? SearchId { get; set; }
    public string? AlbumType { get; set; }

    public static ImportConfiguration Album = new() {
        Type = ImportType.Album
    };
}