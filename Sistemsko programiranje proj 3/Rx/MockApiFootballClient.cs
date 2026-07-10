using Sistemsko_programiranje_proj_3.Models;

namespace Sistemsko_programiranje_proj_3.Rx;

public class MockApiFootballClient : IApiClient
{
    public async Task<ApiFootballResponse> GetStandingsAsync(CancellationToken ct = default)
    {
        return await GetStandingsAsync(39, 2024, ct);
    }

    public async Task<ApiFootballResponse> GetStandingsAsync(int leagueId, int season, CancellationToken ct = default)
    {
        await Task.Delay(100, ct);

        return new ApiFootballResponse(
            Response: new List<LeagueResponseItem>
            {
                new LeagueResponseItem(
                    League: new LeagueDto(
                        Id: leagueId,
                        Name: "Mock League",
                        Standings: new List<List<StandingDto>>
                        {
                            new List<StandingDto>
                            {
                                new StandingDto(
                                    Rank: 1,
                                    Team: new TeamDto(33, "Manchester City", "https://media.api-sports.io/teams/33.png"),
                                    Points: 92,
                                    All: new AllStatsDto(32, 28, 8, 2, new GoalsDto(88, 24))
                                ),
                                new StandingDto(
                                    Rank: 2,
                                    Team: new TeamDto(6, "Liverpool", "https://media.api-sports.io/teams/6.png"),
                                    Points: 86,
                                    All: new AllStatsDto(32, 26, 8, 2, new GoalsDto(86, 24))
                                ),
                                new StandingDto(
                                    Rank: 3,
                                    Team: new TeamDto(42, "Arsenal", "https://media.api-sports.io/teams/42.png"),
                                    Points: 84,
                                    All: new AllStatsDto(32, 25, 9, 2, new GoalsDto(82, 26))
                                ),
                                new StandingDto(
                                    Rank: 4,
                                    Team: new TeamDto(50, "Chelsea", "https://media.api-sports.io/teams/50.png"),
                                    Points: 71,
                                    All: new AllStatsDto(32, 21, 8, 3, new GoalsDto(75, 31))
                                ),
                                new StandingDto(
                                    Rank: 5,
                                    Team: new TeamDto(11, "Manchester United", "https://media.api-sports.io/teams/11.png"),
                                    Points: 69,
                                    All: new AllStatsDto(32, 20, 9, 3, new GoalsDto(71, 29))
                                )
                            }
                        }
                    )
                )
            }
        );
    }
}
