using Akka.Actor;
using Akka.Event;
using Sistemsko_programiranje_proj_3.Actors;
using System;

namespace Sistemsko_programiranje_proj_3
{
    public class LeagueSupervisorActor : ReceiveActor
    {
        //private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly TimeSpan _pollInterval;
        //private readonly Dictionary<(int LeagueId, int Season), IReadOnlyList<TeamTableEntry>> _tables;
        private readonly Dictionary<(int LeagueId, int Season), IActorRef> leagueActors;

        public LeagueSupervisorActor(TimeSpan pollInterval)
        {
            _pollInterval = pollInterval;
            leagueActors = new();
            ConfigureReceivers();
        }

        private void ConfigureReceivers()
        {
            Receive<UpdateStandingsMsg>(HandleUpdateStandings);

            Receive<GetTableQuery>(HandleGetTable);
        }
        private void HandleUpdateStandings(UpdateStandingsMsg msg)
        {

            var key = (msg.LeagueId, msg.Season);

            //var table = msg.Standings
            //    .Select(team => new TeamTableEntry(
            //        Position: team.Position,
            //        TeamName: team.TeamName,
            //        Points: team.Points,
            //        PlayedGames: team.Played,
            //        SuccessPercentage: team.Played == 0
            //            ? 0
            //            : (double)team.Points / (team.Played * 3) * 100
            //    ))
            //    .ToList();


            //_tables[(msg.LeagueId, msg.Season)] = table;

            if (!leagueActors.TryGetValue(key, out var childActor))
            {
                childActor = Context.ActorOf(
                    LeagueActor.CreateProps(
                        msg.LeagueId,
                        msg.Season
                    ),
                    $"league-{msg.LeagueId}-season-{msg.Season}"
                );

                leagueActors[key] = childActor;
            }


            childActor.Tell(msg);
            Console.WriteLine($"Updated table: league={msg.LeagueId}, season={msg.Season}");
        }


        private void HandleGetTable(GetTableQuery msg)
        {
            if (leagueActors.TryGetValue(
                (msg.LeagueId, msg.Season),
                out var childActor))
            {
                childActor.Forward(msg);
                //Sender.Tell(
                //    new TableResponse(
                //        msg.LeagueId,
                //        msg.Season,
                //        childActor
                //    ));
            }
            else
            {
                Sender.Tell(
                    new ErrorMsg(
                        $"No table found for league {msg.LeagueId}, season {msg.Season}"
                    ));
            }
        }


        //public static Props CreateProps(TimeSpan pollInterval)
        //{
        //    return Props.Create(()=> new LeagueSupervisorActor(pollInterval)).WithDispatcher("football-dispatcher");
        //}
        public static Props CreateProps()
        {
            return Props.Create<LeagueSupervisorActor>()
                .WithDispatcher("football-dispatcher");
        }
        //public static Props CreateProps(TimeSpan pollInterval) =>
        //    Props.Create(() => new LeagueSupervisorActor(pollInterval))
        //        .WithDispatcher("football-dispatcher");
    }
}