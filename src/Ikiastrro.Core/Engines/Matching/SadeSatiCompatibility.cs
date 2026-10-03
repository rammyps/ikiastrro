using Ikiastrro.Core.Models;

namespace Ikiastrro.Core.Engines.Matching;

/// <summary>One continuous run of a Saturn period from the Moon, with the retrograde re-entries that split
/// it into several stored intervals merged back together.</summary>
public sealed record SaturnSpell(string Category, DateTime StartUtc, DateTime EndUtc);

/// <summary>Where one person stands in a Saturn category on the as-of date.</summary>
/// <param name="Phase">For Sade Sati only: "Rising", "Peak" or "Setting"; null otherwise or when not in a spell.</param>
public sealed record SaturnStanding(string Category, SaturnSpell? Current, string? Phase, SaturnSpell? Next);

public sealed record SpellOverlap(string Category, DateTime StartUtc, DateTime EndUtc)
{
    public int Days => (int)Math.Round((EndUtc - StartUtc).TotalDays);
}

/// <summary>The two people's standing and overlaps in each Saturn category.</summary>
public sealed record SaturnComparison(
    DateTime AsOfUtc, DateTime HorizonUtc,
    IReadOnlyList<SaturnStanding> First, IReadOnlyList<SaturnStanding> Second,
    IReadOnlyList<SpellOverlap> Overlaps)
{
    public bool SadeSatiOverlaps => Overlaps.Any(o => o.Category == SadeSatiCompatibility.SadeSati);
}

/// <summary>
/// Compares two people's Saturn periods counted from the Moon, from tvf_Chart_SadeSatiPeriods
/// (migration 023): Sade Sati (Saturn over the 12th, 1st and 2nd from the Moon: Rising, Peak, Setting),
/// Kantaka Shani (4th) and Ashtama Shani (8th). Each stored category is split wherever Saturn turns
/// retrograde, so intervals closer than <see cref="MergeGapDays"/> are joined into one spell. The overlap of
/// two spells is simply where both people are inside one at once; the sources do not give a verdict on
/// overlapping Sade Sati, so none is given here. Pure; no I/O.
/// </summary>
public static class SadeSatiCompatibility
{
    public const string SadeSati = "Sade Sati";
    public const string Kantaka = "Kantaka Shani (4th from Moon)";
    public const string Ashtama = "Ashtama Shani (8th from Moon)";

    /// <summary>Gaps shorter than this are retrograde re-entries inside one spell; spells of one category
    /// are about 27 years apart.</summary>
    public const int MergeGapDays = 3 * 365;

    public static SaturnComparison Compare(
        IReadOnlyList<SadeSatiPeriod> first, IReadOnlyList<SadeSatiPeriod> second, DateTime asOfUtc, int horizonYears = 30)
    {
        var horizon = asOfUtc.AddYears(horizonYears);
        var a = Spells(first);
        var b = Spells(second);

        var overlaps = new List<SpellOverlap>();
        foreach (var category in new[] { SadeSati, Kantaka, Ashtama })
        {
            foreach (var x in a.Where(s => s.Category == category))
            foreach (var y in b.Where(s => s.Category == category))
            {
                var start = x.StartUtc > y.StartUtc ? x.StartUtc : y.StartUtc;
                var end = x.EndUtc < y.EndUtc ? x.EndUtc : y.EndUtc;
                if (end > start && end > asOfUtc && start < horizon)
                    overlaps.Add(new SpellOverlap(category, start < asOfUtc ? asOfUtc : start, end > horizon ? horizon : end));
            }
        }

        return new SaturnComparison(asOfUtc, horizon,
            Standing(first, a, asOfUtc), Standing(second, b, asOfUtc),
            overlaps.OrderBy(o => o.StartUtc).ToList());
    }

    private static string? CategoryOf(string periodType) => periodType switch
    {
        var t when t.StartsWith("SadeSati", StringComparison.Ordinal) => SadeSati,
        "KantakaShani" => Kantaka,
        "AshtamaShani" => Ashtama,
        _ => null,
    };

    /// <summary>Stored intervals merged into spells, per category.</summary>
    public static IReadOnlyList<SaturnSpell> Spells(IReadOnlyList<SadeSatiPeriod> periods)
    {
        var spells = new List<SaturnSpell>();
        var byCategory = periods
            .Where(p => p.StartDateTimeUtc is not null && p.EndDateTimeUtc is not null && CategoryOf(p.PeriodType) is not null)
            .GroupBy(p => CategoryOf(p.PeriodType)!);

        foreach (var group in byCategory)
        {
            DateTime? start = null, end = null;
            foreach (var p in group.OrderBy(p => p.StartDateTimeUtc))
            {
                if (start is null) { start = p.StartDateTimeUtc; end = p.EndDateTimeUtc; continue; }
                if ((p.StartDateTimeUtc!.Value - end!.Value).TotalDays <= MergeGapDays)
                    end = p.EndDateTimeUtc > end ? p.EndDateTimeUtc : end;
                else
                {
                    spells.Add(new SaturnSpell(group.Key, start.Value, end.Value));
                    start = p.StartDateTimeUtc; end = p.EndDateTimeUtc;
                }
            }
            if (start is not null) spells.Add(new SaturnSpell(group.Key, start.Value, end!.Value));
        }
        return spells.OrderBy(s => s.StartUtc).ToList();
    }

    private static IReadOnlyList<SaturnStanding> Standing(
        IReadOnlyList<SadeSatiPeriod> periods, IReadOnlyList<SaturnSpell> spells, DateTime asOf) =>
        new[] { SadeSati, Kantaka, Ashtama }.Select(category =>
        {
            var own = spells.Where(s => s.Category == category).ToList();
            var current = own.FirstOrDefault(s => s.StartUtc <= asOf && asOf < s.EndUtc);
            var next = own.FirstOrDefault(s => s.StartUtc > asOf);
            return new SaturnStanding(category, current, category == SadeSati && current is not null ? PhaseAt(periods, current, asOf) : null, next);
        }).ToList();

    // The phase of the stored Sade Sati interval holding the date; inside a retrograde gap, the latest phase begun.
    private static string? PhaseAt(IReadOnlyList<SadeSatiPeriod> periods, SaturnSpell spell, DateTime asOf)
    {
        var inSpell = periods
            .Where(p => p.PeriodType.StartsWith("SadeSati", StringComparison.Ordinal)
                        && p.StartDateTimeUtc >= spell.StartUtc && p.EndDateTimeUtc <= spell.EndUtc)
            .OrderBy(p => p.StartDateTimeUtc).ToList();
        var hit = inSpell.FirstOrDefault(p => p.StartDateTimeUtc <= asOf && asOf < p.EndDateTimeUtc)
                  ?? inSpell.LastOrDefault(p => p.StartDateTimeUtc <= asOf);
        return hit?.PeriodType switch
        {
            var t when t is not null && t.EndsWith("Rising", StringComparison.Ordinal) => "Rising",
            var t when t is not null && t.EndsWith("Peak", StringComparison.Ordinal) => "Peak",
            var t when t is not null && t.EndsWith("Setting", StringComparison.Ordinal) => "Setting",
            _ => null,
        };
    }
}
