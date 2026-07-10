using Akka.Actor;
using Akka.Event;
using Sistemsko_programiranje_proj_3.Actors;
using System;

namespace Sistemsko_programiranje_proj_3
{
    public class LeagueSupervisorActor : ReceiveActor
    {
        //private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly Dictionary<(int LeagueId, int Season), IActorRef> leagueActors;

        public LeagueSupervisorActor()
        {
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
            Console.WriteLine($"[LeagueSupervisor] GetTable traži: league={msg.LeagueId}, season={msg.Season}");
            Console.WriteLine($"[LeagueSupervisor] Dostupni ključevi: {string.Join(", ", leagueActors.Keys)}");
            
            if (leagueActors.TryGetValue(
                (msg.LeagueId, msg.Season),
                out var childActor))
            {
                Console.WriteLine($"[LeagueSupervisor] Pronašao child actor, forward-ujem");
                childActor.Forward(msg);
            }
            else
            {
                Console.WriteLine($"[LeagueSupervisor] NIJE pronašao child actor! Vraćam praznu listu");
                Sender.Tell(
                    new TableResponse(
                        msg.LeagueId,
                        msg.Season,
                        new List<TeamTableEntry>()
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