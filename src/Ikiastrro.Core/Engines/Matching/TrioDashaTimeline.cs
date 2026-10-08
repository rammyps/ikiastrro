namespace Ikiastrro.Core.Engines.Matching;

/// <summary>
/// The windows in which every person given runs the same Mahadasha (level 1) or Antardasha (level 2) lord at
/// the same time: the family's shared periods, generalising <see cref="PairDashaTimeline"/>'s pair windows to
/// any number of people (a father, a mother and a child). Facts only; no period is called supportive or
/// difficult (docs/architecture/compatibility_similarity.md, section 9.3). Pure; no I/O.
/// </summary>
public static class TrioDashaTimeline
{
    public static IReadOnlyList<SharedDashaWindow> SharedByAll(IReadOnlyList<DashaPerson> people, DateTime asOf, DateTime horizon)
    {
        var windows = new List<SharedDashaWindow>();
        if (people.Count < 2) return windows;

        foreach (var level in new[] { 1, 2 })
        {
            var lords = Spans(people[0], level).Select(s => s.Lord).Distinct();
            foreach (var lord in lords)
            {
                // Intersect the lord's spans across everyone, one person at a time.
                var common = new List<(DateTime Start, DateTime End)> { (asOf, horizon) };
                foreach (var p in people)
                {
                    var theirs = Spans(p, level).Where(s => s.Lord == lord).ToList();
                    common = common
                        .SelectMany(c => theirs.Select(t => (Start: c.Start > t.Start ? c.Start : t.Start, End: c.End < t.End ? c.End : t.End)))
                        .Where(w => w.Start < w.End)
                        .ToList();
                    if (common.Count == 0) break;
                }
                windows.AddRange(common.Select(c => new SharedDashaWindow(level, lord, c.Start, c.End)));
            }
        }
        return windows.OrderBy(w => w.Start).ThenBy(w => w.Level).ToList();
    }

    // Every span of the level the person has, running or upcoming, in start order without repeats.
    private static List<DashaSpan> Spans(DashaPerson p, int level) =>
        p.Running.Concat(p.Upcoming).Where(s => s.Level == level)
            .GroupBy(s => (s.Start, s.Lord)).Select(g => g.First()).OrderBy(s => s.Start).ToList();
}
