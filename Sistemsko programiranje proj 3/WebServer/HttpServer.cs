using Akka.Actor;
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

        public HttpServer(IActorRef actorRefParam)
        {
            actorRef = actorRefParam;
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:5000/");
        }



        public async void Start()
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


        private async Task HandleRequest(HttpListenerContext context)
        {

        }


    }
}
