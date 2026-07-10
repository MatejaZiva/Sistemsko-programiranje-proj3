//svi message tipovi kao immutable record-i
using Sistemsko_programiranje_proj_3.Models;
namespace Sistemsko_programiranje_proj_3.Actors;

public record UpdateStandingsMsg(
    int LeagueId,
    int Season,
    IReadOnlyList<TeamStanding> Standings);
public record GetTableQuery(int LeagueId, int Season);
public record TeamTableEntry(
    int Position,
    string TeamName,
    int Points,
    int PlayedGames,
    double SuccessPercentage   // Points / (PlayedGames * 3) * 100
);
public record TableResponse(int LeagueId, int Season, IReadOnlyList<TeamTableEntry> Table);
public record ErrorMsg(string Reason);