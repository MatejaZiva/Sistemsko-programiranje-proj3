using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using IScheduler = System.Reactive.Concurrency.IScheduler;
using Sistemsko_programiranje_proj_3.Actors;


namespace Sistemsko_programiranje_proj_3.Rx
{
    internal class RxPollingService
    {
        private IDisposable? subscription;

        private readonly IActorRef leagueActor;
        private readonly IScheduler scheduler;
        private readonly IApiClient apiClient;


        public RxPollingService(IActorRef leagueActor, IApiClient apiClient)
        {
            this.leagueActor = leagueActor;
            this.apiClient = apiClient;
            // Rx koristi ThreadPool za izvršavanje
            scheduler = TaskPoolScheduler.Default;
        }


        public void Start(int leagueId, int season, TimeSpan pollingInterval)
        {
            subscription =
                StandingsObservable
                    .Create(apiClient, leagueId, season, pollingInterval, scheduler)
                    .Subscribe(
                        standings =>
                        {
                            Console.WriteLine(
                                $"[{DateTime.Now:HH:mm:ss}] [Rx] Emitovani podaci za ligu {leagueId}, sezona {season}"
                            );

                            leagueActor.Tell(
                                new UpdateStandingsMsg(
                                    leagueId,
                                    season,
                                    standings
                                )
                            );
                        },
                        error =>
                        {
                            Console.WriteLine(
                                $"[{DateTime.Now:HH:mm:ss}] [Rx] ERROR: {error.Message}"
                            );
                        }
                    );
        }


        public void Stop()
        {
            subscription?.Dispose();
        }
        
    }
}
