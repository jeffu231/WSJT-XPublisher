using MessagePublisher.Models;
using Microsoft.Net.Http.Headers;

namespace MessagePublisher.Service;

public class FlexRadioApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _radioId;

    public FlexRadioApiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        var host = config.GetValue<string>("FlexSpot:Host");
        _radioId = config.GetValue<string>("FlexSpot:RadioId") ?? string.Empty;
        _httpClient.BaseAddress = new Uri($"http://{host}/api/frs/v1/");
        httpClient.DefaultRequestHeaders.Add(
            HeaderNames.Accept, "application/json");
        httpClient.DefaultRequestHeaders.Add(
            HeaderNames.UserAgent, "Wsjtx-Publisher");
    }

    public async Task SendSpotsAsync(IEnumerable<FlexSpot> spots)
    {
        await _httpClient.PostAsJsonAsync($"radio/radios/{_radioId}/spots", spots);
    }
    
    public async Task RemoveSpotAsync(FlexSpot spot)
    {
        await _httpClient.DeleteAsync($"radio/radios/{_radioId}/spots/{spot.Callsign}/{spot.RxFrequency}");
    }
    
    public async Task ClearSpotsAsync()
    {
        await _httpClient.DeleteAsync($"radio/radios/{_radioId}/spots");
    }
}