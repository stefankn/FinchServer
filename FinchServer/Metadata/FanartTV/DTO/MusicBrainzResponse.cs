using System.Text.Json.Serialization;

namespace FinchServer.Metadata.FanartTV.DTO;

public class MusicBrainzResponse {
    
    // - Properties
    
    [JsonPropertyName("relations")]
    public MusicBrainzRelation[]? Relations { get; set; }
}