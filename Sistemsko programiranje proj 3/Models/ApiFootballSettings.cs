namespace Sistemsko_programiranje_proj_3.Models;

public record ApiFootballSettings(
    string BaseUrl,
    string ApiKey,
    int LeagueId,
    int Season,
    int PollingIntervalSeconds
);