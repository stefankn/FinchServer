using FinchServer.Metadata.FanartTV.DTO;
using FinchServer.Database;

namespace FinchServer.Metadata.FanartTV;

public class FanartTvMetadataFetcher(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory
): IMetadataFetcher {
    
    // - Private Properties

    private readonly Uri _baseUrl = new("https://webservice.fanart.tv");
    private readonly Uri _musicBrainzBaseUrl = new("https://musicbrainz.org");

    
    // - Functions

    public async Task<ArtistMetadata> FetchArtistArtwork(int discogsArtistId) {
        var apiKey = configuration["FANARTTV_APIKEY"];
        if (string.IsNullOrEmpty(apiKey)) throw new Exception("FANARTTV_APIKEY environment variable not defined");

        var musicBrainzId = await ResolveMusicBrainzId(discogsArtistId);
        if (string.IsNullOrEmpty(musicBrainzId)) throw new Exception("Artist not found");

        var client = httpClientFactory.CreateClient();
        client.BaseAddress = _baseUrl;

        var response = await client.GetFromJsonAsync<ArtistResponse>($"/v3/music/{musicBrainzId}?api_key={apiKey}");
        if (response == null) throw new Exception("Invalid response");

        var artworks = new List<ArtistMetadata.Artwork>();
        artworks.AddRange((response.Artistbackground ?? []).Select(i => new ArtistMetadata.Artwork { Url = i.Url, Type = ImageType.Background }));
        artworks.AddRange((response.Artistthumb ?? []).Select(i => new ArtistMetadata.Artwork { Url = i.Url, Type = ImageType.Image }));

        var logos = response.Hdmusiclogo ?? response.Musiclogo ?? [];
        artworks.AddRange(logos.Select(i => new ArtistMetadata.Artwork { Url = i.Url, Type = ImageType.Logo }));

        return new ArtistMetadata {
            Id = discogsArtistId,
            MusicBrainzId = musicBrainzId,
            Name = response.Name,
            Artworks = artworks.ToArray()
        };
    }
    
    
    // - Private Functions
    
    private async Task<string?> ResolveMusicBrainzId(int discogsArtistId) {
        var client = httpClientFactory.CreateClient();
        client.BaseAddress = _musicBrainzBaseUrl;
        client.DefaultRequestHeaders.Add("User-Agent", "Finch/1.0.0");

        var response = await client.GetFromJsonAsync<MusicBrainzResponse>($"/ws/2/url?resource=https://www.discogs.com/artist/{discogsArtistId}&inc=artist-rels&fmt=json");
        return response?.Relations?.FirstOrDefault()?.Artist?.Id;
    }
}