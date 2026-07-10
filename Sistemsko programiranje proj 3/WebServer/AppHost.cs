//140,39,78
using Akka.Actor;
using Microsoft.Extensions.Configuration;
using Sistemsko_programiranje_proj_3;
using Sistemsko_programiranje_proj_3.Conf;
using Sistemsko_programiranje_proj_3.Models;
using Sistemsko_programiranje_proj_3.Rx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sistemsko_programiranje_proj_3
{
    public class AppHost
    {
        private const bool USE_MOCK = false; // Postavi na false za pravi API

        public async Task RunAsync()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var apiSettings = config.GetSection("ApiFootball").Get<ApiFootballSettings>()
                ?? throw new InvalidOperationException("ApiFootball settings not found");

            var system = ActorSystem.Create("footballTables", SystemConfig.getAkkaConfig());

            // Kreiraj Rx servis
            IApiClient apiClient = USE_MOCK 
                ? new MockApiFootballClient()
                : new ApiFootballClient(new HttpClient(), apiSettings);
            
            var stateActor = system.ActorOf(LeagueSupervisorActor.CreateProps(apiClient), "league-supervisor");
            
            var server = new HttpServer(stateActor, apiClient);
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