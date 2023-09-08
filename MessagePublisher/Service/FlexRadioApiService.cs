using MessagePublisher.Models;
using Microsoft.Net.Http.Headers;

namespace MessagePublisher.Service;

public class FlexRadioService
{
    private readonly HttpClient _httpClient;
    private readonly string _radioId;

    public FlexRadioService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        var host = config.GetValue<string>("FlexSpot:Host");
        var port = config.GetValue<int>("FlexSpot:Port");
        _radioId = config.GetValue<string>("FlexSpot:RadioId", string.Empty) ?? string.Empty;
        _httpClient.BaseAddress = new Uri($"{host}:{port}/api/frs/");
        httpClient.DefaultRequestHeaders.Add(
            HeaderNames.Accept, "application/json");
        httpClient.DefaultRequestHeaders.Add(
            HeaderNames.UserAgent, "Wsjtx-Publisher");
    }

    public async Task SendSpotsAsync(IEnumerable<FlexSpot> spots)
    {
        await _httpClient.PostAsJsonAsync($"radio/radios/{_radioId}/spots", spots);
    }
        
    
}