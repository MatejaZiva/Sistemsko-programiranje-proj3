using Akka.Actor;
using Newtonsoft.Json;
using Sistemsko_programiranje_proj_3.Actors;
using Sistemsko_programiranje_proj_3.Rx;
using Sistemsko_programiranje_proj_3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sistemsko_programiranje_proj_3
{
    internal class HttpServer
    {

        //GET https://v3.football.api-sports.io/standings?league=39&season=2025 HTTP/1.1
        //Host: v3.football.api-sports.io
        //x-apisports-key: TVOJ_API_KLJUC

        private readonly HttpListener _listener;
        private readonly IActorRef actorRef;
        private readonly IApiClient apiClient;

        public HttpServer(IActorRef actorRefParam, IApiClient apiClientParam)
        {
            actorRef = actorRefParam;
            apiClient = apiClientParam;
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:5000/");
        }

        public void Stop() => _listener.Stop();
        public void Close() => _listener.Close();

        public void Start()
        {
            _listener.Start();
        }
      

        public async Task RunAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var context = await _listener.GetContextAsync();

                    _ = HandleRequest(context).ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            Console.WriteLine(
                                $"[{DateTime.Now:HH:mm:ss}] UNHANDLED REQUEST ERROR | " +
                                $"{t.Exception?.GetBaseException().Message}");
                    }, TaskContinuationOptions.OnlyOnFaulted);
                }
            }
            catch (HttpListenerException) when (token.IsCancellationRequested) { }
        }

        private async Task HandleRequest(HttpListenerContext ctx)
        {
            // Preskoči favicon zahteve
            if (ctx.Request.Url?.AbsolutePath == "/favicon.ico")
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                ctx.Response.Close();
                return;
            }

            if (ctx.Request.HttpMethod != "GET")
            {
                await WriteJson(ctx, HttpStatusCode.MethodNotAllowed,
                    new { error = "Only GET requests are supported." });
                return;
            }
            var league = ctx.Request.QueryString["league"];
            var season = ctx.Request.QueryString["season"];
            var requestId = Guid.NewGuid().ToString("N")[..8];

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ── REQUEST [{requestId}] ──────────────────────");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Method: {ctx.Request.HttpMethod} | URL: {ctx.Request.Url} | Thread: {Thread.CurrentThread.ManagedThreadId}");

            try
            {
                if (string.IsNullOrWhiteSpace(league) || string.IsNullOrWhiteSpace(season))
                {
                    await WriteJson(ctx, HttpStatusCode.BadRequest,
                        new { error = "Query parameters are required." });
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] BAD REQUEST [{requestId}] | Missing parameters");
                    return;
                }


                var startTime = DateTime.Now;
                var result = await actorRef.Ask<TableResponse>(
                    new GetTableQuery(int.Parse(league), int.Parse(season)),
                    TimeSpan.FromSeconds(15));
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;

                // Ako nema podatka u aktorima, pokušaj direktno sa API-jem
                if (result.Table.Count == 0)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] NO DATA IN ACTORS [{requestId}], fetching from API...");
                    try
                    {
                        var apiResponse = await apiClient.GetStandingsAsync();
                        var standings = DataMapper.MapToTeamStandings(apiResponse);
                        
                        // Emituj aktorima za keš
                        actorRef.Tell(new UpdateStandingsMsg(int.Parse(league), int.Parse(season), standings));
                        
                        // Mapuj na TableEntry
                        var table = standings
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

                        result = new TableResponse(int.Parse(league), int.Parse(season), table);
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] API FALLBACK SUCCESS [{requestId}]");
                    }
                    catch (Exception apiEx)
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] API FALLBACK ERROR [{requestId}]: {apiEx.Message}");
                    }
                }

                {
                    await WriteJson(ctx, HttpStatusCode.OK, new
                    {
                        league = result.LeagueId,
                        season = result.Season,
                        count = result.Table.Count,
                        standings = result.Table
                    }, indented: true);

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] SUCCESS [{requestId}] | Duration: {elapsed:F0}ms");
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine(
                    $"[{DateTime.Now:HH:mm:ss}] TIMEOUT [{requestId}]");

                await WriteJson(ctx, HttpStatusCode.ServiceUnavailable, new
                {
                    error = "Podaci se još učitavaju, pokušaj ponovo za par sekundi.",

                    league,
                    season,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[{DateTime.Now:HH:mm:ss}] ERROR [{requestId}] | {ex.GetType().Name}: {ex.Message}");

                await WriteJson(ctx, HttpStatusCode.InternalServerError, new
                {
                    error = ex.Message,
                    retry = false
                });
            }
            finally
            {
                ctx.Response.Close();
            }
        }

        private static async Task WriteJson(HttpListenerContext ctx, HttpStatusCode status, object payload, bool indented = false)
        {
            var json = JsonConvert.SerializeObject(payload, indented ? Formatting.Indented : Formatting.None);
            var bytes = Encoding.UTF8.GetBytes(json);
            ctx.Response.StatusCode = (int)status;
            ctx.Response.ContentType = "application/json; charset=utf-8";
            await ctx.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        }
    }


}

