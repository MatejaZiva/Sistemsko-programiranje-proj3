

using Akka.Actor;
using Sistemsko_programiranje_proj_3;
using Sistemsko_programiranje_proj_3.Conf;
using Sistemsko_programiranje_proj_3.Rx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Sistemsko_programiranje_proj_3.Rx.RxPollingService;

namespace Sistemsko_programiranje_proj_3
{
    public class AppHost
    {
        public async Task RunAsync()
        {
            var system = ActorSystem.Create("footballTables", SystemConfig.getAkkaConfig());
            


            var pollInterval = TimeSpan.FromMinutes(2);
            var stateActor = system.ActorOf(LeagueSupervisorActor.CreateProps(), "league-supervisor");

            //ovo bi trebalo se uradi ali kasnije zato sto bi prvo da radi ovo
            //var rxService = new RxPollingService(stateActor);
            //var stateActor = system.ActorOf(LeagueSupervisorActor.CreateProps(pollInterval), "state");
            
            var server = new HttpServer(stateActor);
            server.Start();

            var cts = new CancellationTokenSource();
            RegisterShutdownHandlers(server, cts);
            await server.RunAsync(cts.Token);

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] SERVER STOPPING...");
            server.Close();
            await system.Terminate();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] SERVER STOPPED. Goodbye.");
        }

        private static void RegisterShutdownHandlers(HttpServer server, CancellationTokenSource cts)
        {
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] SHUTDOWN SIGNAL (Ctrl+C)");
                cts.Cancel();
                server.Stop();
            };
            _ = Task.Run(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    var line = Console.ReadLine();
                    if (line?.Equals("q", StringComparison.OrdinalIgnoreCase) == true ||
                        line?.Equals("exit", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] SHUTDOWN SIGNAL ('q')");
                        cts.Cancel();
                        server.Stop();
                        break;
                    }
                }
            });
        }
    }
}