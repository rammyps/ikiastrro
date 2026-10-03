using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dignity;
using Ikiastrro.Core.Engines.Houses;

namespace Ikiastrro.Core.Engines.Matching;

/// <summary>One stored Vimshottari period (level 1 Maha, 2 Antar, 3 Pratyantar) with its dates.</summary>
public sealed record DashaSpan(int Level, string Lord, DateTime Start, DateTime End);

/// <summary>One person's dasha inputs for the pair page: the Moon-nakshatra (birth) dasha lord, the
/// periods running on the as-of date, and the Maha and Antar spans from the as-of date to the horizon.</summary>
public sealed record DashaPerson(
    string Name, string BirthLord, IReadOnlyList<DashaSpan> Running, IReadOnlyList<DashaSpan> Upcoming,
    DoshaChart Chart, PlanetName? Darakaraka);

/// <summary>Facts about one running dasha lord in its own chart. No good-or-bad verdict: the sources that
/// would classify supportive and strained periods are not yet cited, so only what the chart says is stated.</summary>
public sealed record DashaLordFacts(
    string Lord, int Level, DateTime End, IReadOnlyList<int> Rules, int? PlacedIn, bool IsDarakaraka, bool IsVenusOrJupiter);

public sealed record DashaAttitude(string From, string To, string? Attitude);

/// <summary>Two people's dasha picture side by side.</summary>
public sealed record DashaPairReading(
    DateTime AsOf, DateTime Horizon,
    DashaAttitude BirthLordFirstToSecond, DashaAttitude BirthLordSecondToFirst,
    DashaAttitude? MahaFirstToSecond, DashaAttitude? MahaSecondToFirst,
    IReadOnlyList<DashaLordFacts> FirstFacts, IReadOnlyList<DashaLordFacts> SecondFacts,
    IReadOnlyList<string> SandhiNotes);

/// <summary>
/// Compares two people's Vimshottari dashas from what is stored: the birth-dasha lords and how each regards
/// the other (permanent friendship, <see cref="DignityEngine.NaturalAttitude"/>), the running Maha, Antar and
/// Pratyantar lords, each running lord's lordships, house and Darakaraka role in its own chart, and any
/// Mahadasha junction close to the as-of date. Facts only. Pure; no I/O.
/// </summary>
public static class DashaCompatibility
{
    /// <summary>A Mahadasha ending or starting within this many days of the as-of date is reported. The width
    /// is a display choice, not a rule from a source.</summary>
    public const int JunctionWindowDays = 365;

    public static DashaPairReading Compare(DashaPerson first, DashaPerson second, DateTime asOf, DateTime horizon)
    {
        var firstMaha = first.Running.FirstOrDefault(s => s.Level == 1);
        var secondMaha = second.Running.FirstOrDefault(s => s.Level == 1);

        return new DashaPairReading(
            asOf, horizon,
            Attitude(first.BirthLord, second.BirthLord), Attitude(second.BirthLord, first.BirthLord),
            firstMaha is not null && secondMaha is not null ? Attitude(firstMaha.Lord, secondMaha.Lord) : null,
            firstMaha is not null && secondMaha is not null ? Attitude(secondMaha.Lord, firstMaha.Lord) : null,
            first.Running.Select(s => Facts(s, first)).ToList(),
            second.Running.Select(s => Facts(s, second)).ToList(),
            Sandhi(first, asOf).Concat(Sandhi(second, asOf)).ToList());
    }

    public static DashaAttitude Attitude(string from, string to) =>
        new(from, to, DignityEngine.NaturalAttitude(from, to));

    public static DashaLordFacts Facts(DashaSpan span, DashaPerson person)
    {
        var rules = new List<int>();
        int? placedIn = null;
        if (Enum.TryParse<PlanetName>(span.Lord, out var planet))
        {
            for (var house = 1; house <= 12; house++)
                if (HouseEngine.GetSignLord(HouseEngine.GetHouseSign(person.Chart.Lagna, house)) == span.Lord)
                    rules.Add(house);
            if (person.Chart.Signs.TryGetValue(planet, out var sign))
                placedIn = DoshaSamyaCalculator.HouseFrom(person.Chart.Lagna, sign);
        }
        return new DashaLordFacts(
            span.Lord, span.Level, span.End, rules, placedIn,
            person.Darakaraka is { } dk && dk.ToString() == span.Lord,
            span.Lord is "Venus" or "Jupiter");
    }

    private static IEnumerable<string> Sandhi(DashaPerson person, DateTime asOf)
    {
        var running = person.Running.FirstOrDefault(s => s.Level == 1);
        if (running is null) yield break;

        var daysToEnd = (running.End - asOf).TotalDays;
        if (daysToEnd is >= 0 and <= JunctionWindowDays)
            yield return $"{person.Name}'s {running.Lord} Mahadasha ends {running.End:d MMM yyyy}, within a year.";

        var daysSinceStart = (asOf - running.Start).TotalDays;
        if (daysSinceStart is >= 0 and <= JunctionWindowDays)
            yield return $"{person.Name}'s {running.Lord} Mahadasha began {running.Start:d MMM yyyy}, within the last year.";
    }
}
