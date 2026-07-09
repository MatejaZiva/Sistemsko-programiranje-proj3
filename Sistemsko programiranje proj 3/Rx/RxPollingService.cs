using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using IScheduler = System.Reactive.Concurrency.IScheduler;


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


        public void Start()
        {
            //subscription =
            //    Observable
            //        .Interval(
            //            TimeSpan.FromMinutes(5),
            //            scheduler
            //        )
            //        .StartWith(0)
            //        .SelectMany(_ =>
            //        {
            //            ovde se poziva API
            //            return StandingsObservable
            //                .GetLeagueStandings();
            //        })
            //        .Subscribe(
            //            standings =>
            //            {
            //                Console.WriteLine(
            //                    $"[{DateTime.Now}] Rx dobio podatke"
            //                );


            //                supervisorActor.Tell(
            //                    new UpdateStandingsMessage(
            //                        standings
            //                    )
            //                );
            //            },
            //            error =>
            //            {
            //                Console.WriteLine(
            //                    $"Rx ERROR: {error.Message}"
            //                );
            //            }
            //        );
        }


        public void Stop()
        {
            subscription?.Dispose();
        }
        
    }
}
