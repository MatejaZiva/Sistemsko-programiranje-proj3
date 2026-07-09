//klase za deserializaciju API odgovora
namespace Sistemsko_programiranje_proj_3.Models;

public record ApiFootballResponse(List<LeagueResponseItem> Response);

public record LeagueResponseItem(LeagueDto League);

public record LeagueDto(int Id, string Name, List<List<StandingDto>> Standings);

public record StandingDto(
    int Rank,
    TeamDto Team,
    int Points,
    AllStatsDto All
);

public record TeamDto(int Id, string Name, string Logo);

public record AllStatsDto(int Played, int Win, int Draw, int Lose, GoalsDto Goals);

public record GoalsDto(int For, int Against);