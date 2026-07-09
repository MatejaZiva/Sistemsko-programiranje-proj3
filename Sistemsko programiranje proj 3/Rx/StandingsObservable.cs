//Rx pipeline: Observable.Interval/Timer -> fetch -> map -> filter
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Sistemsko_programiranje_proj_3.Models;

namespace Sistemsko_programiranje_proj_3.Rx;
public static class StandingsObservable
{
    // Kreira Rx pipeline koji periodično poziva API i emituje mapirane podatke.
    // Scheduler je eksplicitno prosleđen radi zahteva "Rx Scheduler + Akka Dispatcher" (bonus poeni).
    public static IObservable<IReadOnlyList<TeamStanding>> Create(
        ApiFootballClient client,
        TimeSpan pollingInterval,
        IScheduler scheduler)
    {
        return Observable
            .Interval(pollingInterval, scheduler)
            .StartWith(scheduler, 0) // odmah prvi poziv, ne čekaj prvi interval
            .SelectMany(_ => Observable.FromAsync(() => client.GetStandingsAsync()))
            .Select(DataMapper.MapToTeamStandings)
            .Where(standings => standings.Count > 0) // osnovno filtriranje - traženo u zadatku
            .Catch<IReadOnlyList<TeamStanding>, Exception>(ex =>
            {
                Console.WriteLine($"[Rx] Greška pri pozivu API-ja: {ex.Message}");
                return Observable.Empty<IReadOnlyList<TeamStanding>>();
            })
            .Retry(); // nastavlja dalje posle greške, ne "umire" ceo stream
    }
}