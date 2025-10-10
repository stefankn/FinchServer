using System.Text.Json.Serialization;

namespace FinchServer.Metadata.FanartTV.DTO;

public class ArtistResponse {
    
    // - Types

    public class Image {
        
        [JsonPropertyName("url")]
        public required string Url { get; set; }
    }
    
    
    // - Properties
    
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    [JsonPropertyName("mbid_id")]
    public required string MbId { get; set; }
    
    [JsonPropertyName("artistbackground")]
    public Image[]? Artistbackground { get; set; }
    
    [JsonPropertyName("artistthumb")]
    public Image[]? Artistthumb { get; set; }
    
    [JsonPropertyName("musiclogo")]
    public Image[]? Musiclogo { get; set; }
    
    [JsonPropertyName("hdmusiclogo")]
    public Image[]? Hdmusiclogo { get; set; }
}