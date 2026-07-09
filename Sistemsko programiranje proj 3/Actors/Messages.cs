//svi message tipovi kao immutable record-i
using Sistemsko_programiranje_proj_3.Models;
namespace Sistemsko_programiranje_proj_3.Actors;

public record UpdateStandingsMsg(IReadOnlyList<TeamStanding> Standings);
public record UpdateTeamStateMsg(TeamStanding Standing);
public record GetTableQuery;
public record GetTeamQuery(int TeamId);
public record TeamTableEntry(
    int Position,
    string TeamName,
    int Points,
    int PlayedGames,
    double SuccessPercentage   // Points / (PlayedGames * 3) * 100
);
public record TableResponse(IReadOnlyList<TeamTableEntry> Table);
public record TeamResponse(TeamTableEntry? Entry);
public record ErrorMsg(string Reason);