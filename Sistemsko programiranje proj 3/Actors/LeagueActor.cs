using Akka.Actor;
using Sistemsko_programiranje_proj_3.Rx;
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
        private RxPollingService? _pollingService;
        private IApiClient apiClient;

        public LeagueActor(int leagueId, int season, IApiClient apiClient)
        {
            _leagueId = leagueId;
            _season = season;
            this.apiClient = apiClient;
            Receive<UpdateStandingsMsg>(HandleUpdate);
            Receive<GetTableQuery>(HandleGet);
        }

        protected override void PreStart()
        {
            Console.WriteLine(
                $"[Child] Starting {_leagueId}/{_season}"
            );

            _pollingService = new RxPollingService(Self, apiClient);

            _pollingService.Start(
                _leagueId,
                _season,
                TimeSpan.FromSeconds(5)
            );
        }

        protected override void PostStop()
        {
            Console.WriteLine(
                $"[Child] Stopping {_leagueId}/{_season}"
            );

            _pollingService?.Stop();
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

            Console.WriteLine($"[Child] Updated {_leagueId}/{_season}");
            Console.WriteLine($"[Child] Updated {_leagueId}/{_season}, count={_table.Count}"
);
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


        public static Props CreateProps(
        int leagueId,
        int season,
        IApiClient apiClient)
        {
            return Props.Create(() => new LeagueActor(leagueId, season, apiClient)).WithDispatcher("football-dispatcher");
        }
    }
}

