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
        private readonly IActorRef supervisorActor;

        private IDisposable? subscription;

        private readonly IScheduler scheduler;


        public RxPollingService(IActorRef supervisorActor)
        {
            this.supervisorActor = supervisorActor;

            // Rx koristi ThreadPool za izvršavanje
            scheduler = TaskPoolScheduler.Default;
        }


        public void Start(IApiClient client, int leagueId, int season, TimeSpan pollingInterval)
        {
            subscription =
                StandingsObservable
                    .Create(client, pollingInterval, scheduler)
                    .Subscribe(
                        standings =>
                        {
                            Console.WriteLine(
                                $"[{DateTime.Now:HH:mm:ss}] [Rx] Emitovani podaci za ligu {leagueId}, sezona {season}"
                            );

                            supervisorActor.Tell(
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
