//Rx pipeline: Observable.Interval/Timer -> fetch -> map -> filter
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Sistemsko_programiranje_proj_3.Models;
namespace Sistemsko_programiranje_proj_3.Rx;
public static class StandingsObservable
{
    // Kreira Rx pipeline koji periodično poziva API i emituje mapirane podatke.
    // Scheduler je eksplicitno prosleđen.
    public static IObservable<IReadOnlyList<TeamStanding>> Create(
        IApiClient client,
        int leagueId,
        int season,
        TimeSpan pollingInterval,
        IScheduler scheduler)
    {
        return Observable
            .Interval(pollingInterval, scheduler)
            .StartWith(scheduler, 0) // odma prvi poziv, ne čekam prvi interval
            .SelectMany(_ => Observable.FromAsync(() => client.GetStandingsAsync(leagueId, season)))
            .Select(DataMapper.MapToTeamStandings)
            .Where(standings => standings.Count > 0) // sprecim deljenje sa 0
            .Catch<IReadOnlyList<TeamStanding>, Exception>(ex =>
            {
                Console.WriteLine($"[Rx] Greška pri pozivu API-ja: {ex.Message}");
                return Observable.Empty<IReadOnlyList<TeamStanding>>();
            })
            .Retry(); // nastavlja dalje posle grešku
    }
}