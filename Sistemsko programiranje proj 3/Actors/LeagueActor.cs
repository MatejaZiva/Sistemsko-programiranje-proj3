using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistemsko_programiranje_proj_3.Actors
{
    internal class LeagueActor : ReceiveActor
    {

        private readonly int _leagueId;
        private readonly int _season;

        private IReadOnlyList<TeamTableEntry> _table = new List<TeamTableEntry>();


        public LeagueActor(int leagueId, int season)
        {
            _leagueId = leagueId;
            _season = season;

            Receive<UpdateStandingsMsg>(HandleUpdate);
            Receive<GetTableQuery>(HandleGet);
        }


        private void HandleUpdate(UpdateStandingsMsg msg)
        {
            _table = msg.Standings
                .Select(team => new TeamTableEntry(
                    Position: team.Position,
                    TeamName: team.TeamName,
                    Points: team.Points,
                    PlayedGames: team.Played,
                    SuccessPercentage: team.Played == 0
                        ? 0
                        : (double)team.Points / (team.Played * 3) * 100
                ))
                .ToList();

            Console.WriteLine(
                $"[Child] Updated {_leagueId}/{_season}");
        }


        private void HandleGet(GetTableQuery msg)
        {
            Sender.Tell(
                new TableResponse(
                    _leagueId,
                    _season,
                    _table
                )
            );
        }


        public static Props CreateProps(int leagueId, int season)
        {
            return Props.Create(() => new LeagueActor(leagueId, season)).WithDispatcher("football-dispatcher");
        }
    }
}
