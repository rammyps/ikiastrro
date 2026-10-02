namespace Ikiastrro.Web.Components.Charts;

/// <summary>One part of a stacked bar before layout: a value (any sign) and how to draw it.</summary>
public sealed record StackPart(string Key, string Label, string Token, decimal Value, string? Detail = null, int Shade = 0);

/// <summary>A laid-out segment: <see cref="LeftPercent"/>/<see cref="WidthPercent"/> are percentages
/// of the whole track. <see cref="IsNegative"/> segments sit left of zero.</summary>
public sealed record StackSegment(StackPart Part, double LeftPercent, double WidthPercent)
{
    public bool IsNegative => Part.Value < 0;
}

/// <summary>A shared value axis across rows: <see cref="Min"/> is 0 unless some row has a negative
/// part, <see cref="Max"/> covers the longest positive stack and every marker.</summary>
public sealed record StackAxis(decimal Min, decimal Max)
{
    public double Position(decimal value) =>
        Max <= Min ? 0 : (double)((Math.Clamp(value, Min, Max) - Min) / (Max - Min) * 100m);

    /// <summary>Whole-unit ticks from Min to Max, thinned to at most ~10 labels.</summary>
    public IReadOnlyList<decimal> Ticks()
    {
        var span = Max - Min;
        var step = span <= 10 ? 1m : span <= 20 ? 2m : 5m;
        var ticks = new List<decimal>();
        for (var t = Math.Ceiling(Min / step) * step; t <= Max; t += step) ticks.Add(t);
        return ticks;
    }
}

/// <summary>
/// Geometry for a stacked horizontal bar on a shared axis (Astro Facts 3.1 Planet Strength). Positive
/// parts stack rightward from zero in the order given; negative parts (Dṛk Bala can be) stack leftward
/// from zero, so they are drawn instead of hidden. Pure — no rendering — so it is unit-testable.
/// </summary>
public static class StackedBarLayout
{
    /// <summary>Axis covering every row's positive and negative totals and markers. Max rounds up to a
    /// whole unit (at least 1); Min rounds down to a half unit, and is 0 when nothing is negative.</summary>
    public static StackAxis Axis(IEnumerable<IReadOnlyList<StackPart>> rows, IEnumerable<decimal?> markers)
    {
        decimal max = 1, min = 0;
        foreach (var parts in rows)
        {
            max = Math.Max(max, parts.Where(p => p.Value > 0).Sum(p => p.Value));
            min = Math.Min(min, parts.Where(p => p.Value < 0).Sum(p => p.Value));
        }
        foreach (var m in markers) if (m is { } v) max = Math.Max(max, v);
        return new StackAxis(Math.Floor(min * 2) / 2, Math.Ceiling(max));
    }

    public static IReadOnlyList<StackSegment> Layout(IReadOnlyList<StackPart> parts, StackAxis axis)
    {
        var segments = new List<StackSegment>();
        decimal right = 0, left = 0;
        foreach (var p in parts.Where(p => p.Value != 0))
        {
            if (p.Value > 0)
            {
                var start = axis.Position(right);
                right += p.Value;
                segments.Add(new StackSegment(p, start, axis.Position(right) - start));
            }
            else
            {
                var end = axis.Position(left);
                left += p.Value;
                var start = axis.Position(left);
                segments.Add(new StackSegment(p, start, end - start));
            }
        }
        return segments;
    }
}
