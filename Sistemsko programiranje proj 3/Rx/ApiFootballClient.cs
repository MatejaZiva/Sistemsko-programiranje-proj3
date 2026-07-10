//HTTP pozivi ka API-FOOTBALL
using Sistemsko_programiranje_proj_3.Models;
using System.Net.Http.Json;

namespace Sistemsko_programiranje_proj_3.Rx;

public interface IApiClient
{
    Task<ApiFootballResponse> GetStandingsAsync(CancellationToken ct = default);
    Task<ApiFootballResponse> GetStandingsAsync(int leagueId, int season, CancellationToken ct = default);
}

public class ApiFootballClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ApiFootballSettings _settings;

    public ApiFootballClient(HttpClient httpClient, ApiFootballSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;

        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("x-apisports-key", _settings.ApiKey);
    }

    public async Task<ApiFootballResponse> GetStandingsAsync(CancellationToken ct = default)
    {
        return await GetStandingsAsync(_settings.LeagueId, _settings.Season, ct);
    }

    public async Task<ApiFootballResponse> GetStandingsAsync(int leagueId, int season, CancellationToken ct = default)
    {
        var url = $"/standings?league={leagueId}&season={season}";

        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiFootballResponse>(cancellationToken: ct);

        if (result is null)
            throw new InvalidOperationException("API-FOOTBALL vratio prazan odgovor.");

        return result;
    }
}