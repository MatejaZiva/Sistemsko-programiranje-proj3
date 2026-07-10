using Akka.Actor;
using Akka.Event;
using Sistemsko_programiranje_proj_3.Actors;
using Sistemsko_programiranje_proj_3.Rx;
using System;

namespace Sistemsko_programiranje_proj_3
{
    public class LeagueSupervisorActor : ReceiveActor
    {
        //private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly Dictionary<(int LeagueId, int Season), IActorRef> leagueActors;
        private readonly IApiClient apiClient;
        public LeagueSupervisorActor(IApiClient apiClient)
        {
            leagueActors = new();
            this.apiClient = apiClient; 
            ConfigureReceivers();
        }

        private void ConfigureReceivers()
        {
            Receive<UpdateStandingsMsg>(HandleUpdateStandings);

            Receive<GetTableQuery>(HandleGetTable);

            Receive<CreateLeague>(HandleCreateLeague);
        }
        private void HandleUpdateStandings(UpdateStandingsMsg msg)
        {

            var key = (msg.LeagueId, msg.Season);

            if (!leagueActors.TryGetValue(key, out var childActor))
            {
                childActor = Context.ActorOf(
                    LeagueActor.CreateProps(
                        msg.LeagueId,
                        msg.Season,
                        apiClient
                    ),
                    $"league-{msg.LeagueId}-season-{msg.Season}"
                );

                leagueActors[key] = childActor;
            }


            childActor.Tell(msg);
            Console.WriteLine($"Updated table: league={msg.LeagueId}, season={msg.Season}");
        }

        private void HandleCreateLeague(CreateLeague msg)
        {
            var key = (msg.LeagueId, msg.Season);
            
            if (!leagueActors.ContainsKey(key))
            {
                var childActor = Context.ActorOf(
                    LeagueActor.CreateProps(
                        msg.LeagueId,
                        msg.Season,
                        apiClient
                    )
                );

                Console.WriteLine($"Created league-{msg.LeagueId}-season-{msg.Season}");
                leagueActors[key] = childActor;
                Sender.Tell(true);
            }


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

        public static Props CreateProps(IApiClient apiClient)
        {
            return Props.Create(()=> new LeagueSupervisorActor(apiClient))
                .WithDispatcher("football-dispatcher");
        }
    }
}