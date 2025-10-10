using System.Text.Json.Serialization;

namespace FinchServer.Metadata.FanartTV.DTO;

public class MusicBrainzRelation {
    
    // - Types

    public class RelationArtist {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }
    
    
    // - Properties
    
    [JsonPropertyName("artist")]
    public RelationArtist? Artist { get; set; }
}