//mapiranje JSON -> interni model
using Sistemsko_programiranje_proj_3.Models;

namespace Sistemsko_programiranje_proj_3.Rx;
public static class DataMapper
{
    // Uzima sirov API odgovor i vraća listu TeamStanding (immutable) recorda.
    // Standings je List<List<>> zbog grupa (Champions League faza grupa) -
    // za obične lige koristi se samo prva grupa [0].
public static IReadOnlyList<TeamStanding> MapToTeamStandings(ApiFootballResponse apiResponse)
    {
        var leagueItem = apiResponse.Response.FirstOrDefault();
        if (leagueItem is null)
        {
            Console.WriteLine("[DataMapper] API vratio null");
            return Array.Empty<TeamStanding>();
        }

        var standingsGroup = leagueItem.League.Standings.FirstOrDefault();
        if (standingsGroup is null)
        {
            Console.WriteLine($"[DataMapper] Standings je null. Broj grupa: {leagueItem.League.Standings.Count}");
            return Array.Empty<TeamStanding>();
        }

        var result = standingsGroup
            .Select(s => new TeamStanding(
                TeamId: s.Team.Id,
                TeamName: s.Team.Name,
                Position: s.Rank,
                Points: s.Points,
                Played: s.All.Played,
                Win: s.All.Win,
                Draw: s.All.Draw,
                Lose: s.All.Lose,
                GoalsFor: s.All.Goals.For,
                GoalsAgainst: s.All.Goals.Against
            ))
            .ToList();

        Console.WriteLine($"[DataMapper] Mapirao {result.Count} timova");
        return result;
    }
}