namespace Ikiastrro.Core.Engines.Matching;

/// <summary>A Mahadasha (level 1) or Antardasha (level 2) that begins on <see cref="Date"/> for one person, with
/// the lord it replaces and what the new lord rules and where it sits in that person's own chart.</summary>
public sealed record DashaChange(
    DateTime Date, bool IsFirst, string Person, int Level, string FromLord, string ToLord,
    IReadOnlyList<int> Rules, int? PlacedIn);

/// <summary>A stretch of time in which both people are in the same Mahadasha (or Antardasha) lord.</summary>
public sealed record SharedDashaWindow(int Level, string Lord, DateTime Start, DateTime End);

public sealed record DashaTimelineReading(
    IReadOnlyList<DashaChange> Changes, IReadOnlyList<SharedDashaWindow> SharedWindows);

/// <summary>
/// The pair's dasha timing as facts: every Mahadasha and Antardasha change for either person between the
/// as-of date and the horizon (in date order, so it is plain which person each change affects), and the
/// windows in which both people are in the same lord at the same level. Nothing here says a period is
/// supportive or difficult: no source is cited that classifies periods for a marriage, so none is given
/// (docs/architecture/compatibility_similarity.md, section 9.3). Pure; no I/O.
/// </summary>
public static class DashaTimeline
{
    public static DashaTimelineReading Build(DashaPerson first, DashaPerson second, DateTime asOf, DateTime horizon)
    {
        var changes = Changes(first, true, asOf, horizon).Concat(Changes(second, false, asOf, horizon))
            .OrderBy(c => c.Date).ThenBy(c => c.Level).ThenBy(c => c.IsFirst ? 0 : 1).ToList();
        return new DashaTimelineReading(changes, Shared(first, second, asOf, horizon));
    }

    // Every span of the level the person has, running or upcoming, in start order without repeats.
    private static List<DashaSpan> Spans(DashaPerson p, int level) =>
        p.Running.Concat(p.Upcoming).Where(s => s.Level == level)
            .GroupBy(s => (s.Start, s.Lord)).Select(g => g.First()).OrderBy(s => s.Start).ToList();

    private static IEnumerable<DashaChange> Changes(DashaPerson p, bool isFirst, DateTime asOf, DateTime horizon)
    {
        foreach (var level in new[] { 1, 2 })
        {
            var spans = Spans(p, level);
            for (var i = 1; i < spans.Count; i++)
            {
                var next = spans[i];
                if (next.Start <= asOf || next.Start > horizon) continue;
                var facts = DashaCompatibility.Facts(next, p);
                yield return new DashaChange(next.Start, isFirst, p.Name, level, spans[i - 1].Lord, next.Lord, facts.Rules, facts.PlacedIn);
            }
        }
    }

    private static List<SharedDashaWindow> Shared(DashaPerson first, DashaPerson second, DateTime asOf, DateTime horizon)
    {
        var windows = new List<SharedDashaWindow>();
        foreach (var level in new[] { 1, 2 })
            foreach (var a in Spans(first, level))
                foreach (var b in Spans(second, level).Where(b => b.Lord == a.Lord))
                {
                    var start = new[] { a.Start, b.Start, asOf }.Max();
                    var end = new[] { a.End, b.End, horizon }.Min();
                    if (start < end) windows.Add(new SharedDashaWindow(level, a.Lord, start, end));
                }
        return windows.OrderBy(w => w.Start).ThenBy(w => w.Level).ToList();
    }
}
