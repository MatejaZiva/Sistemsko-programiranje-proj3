//record - ime, pozicija, bodovi, odigrane utakmice
namespace Sistemsko_programiranje_proj_3.Models;
public record TeamStanding(
    int TeamId,
    string TeamName,
    int Position,
    int Points,
    int Played,
    int Win,
    int Draw,
    int Lose,
    int GoalsFor,
    int GoalsAgainst
);