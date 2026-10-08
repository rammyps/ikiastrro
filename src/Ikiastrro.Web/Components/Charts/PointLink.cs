using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Web.Components.Charts;

/// <summary>A special point a table row stands for: the D1 sign it falls in (internal ZodiacName
/// spelling, e.g. "Capricornus") and the label shown for it on the chart.</summary>
public sealed record PointFocus(string Sign, string Label)
{
    /// <summary>The sign a nirayana longitude (0–360°) falls in.</summary>
    public static string SignOf(double longitude) =>
        ((ZodiacName)(int)(AstroMath.Normalize(longitude) / 30)).ToString();
}

/// <summary>
/// Links special-point table rows to the chart: hovering (or keyboard-focusing) a row previews its
/// sign on the chart, clicking pins it until clicked again. One instance is cascaded over the
/// Special points tab; the Astro Facts page reads <see cref="Current"/> for the chart highlight.
/// </summary>
public sealed class PointLinkState
{
    public PointFocus? Hover { get; private set; }
    public PointFocus? Pinned { get; private set; }

    /// <summary>What the chart shows: the hovered row, else the pinned one.</summary>
    public PointFocus? Current => Hover ?? Pinned;

    public event Action? Changed;

    public void SetHover(PointFocus? focus)
    {
        if (Equals(Hover, focus)) return;
        Hover = focus;
        Changed?.Invoke();
    }

    public void TogglePin(PointFocus focus)
    {
        Pinned = Equals(Pinned, focus) ? null : focus;
        Changed?.Invoke();
    }

    public void Clear()
    {
        if (Hover is null && Pinned is null) return;
        Hover = Pinned = null;
        Changed?.Invoke();
    }
}
