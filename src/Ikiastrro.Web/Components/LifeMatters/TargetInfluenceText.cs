using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.LifeMatters;

namespace Ikiastrro.Web.Components.LifeMatters;

/// <summary>
/// Customer wording for <see cref="TargetInfluences"/> (PVR step 5). Presentation only: which
/// planet helps or hinders is decided by the engine's lean; this picks the ones worth naming on
/// the answer card and says why in plain words.
/// </summary>
public static class TargetInfluenceText
{
    /// <summary>Planets named on the answer card per side.</summary>
    public const int Named = 2;

    /// <summary>Planets that act on the target (occupy, aspect or intervene) or rule it, and lean
    /// towards supporting it — functional benefics first.</summary>
    public static IReadOnlyList<PlanetInfluence> Helpers(TargetInfluenceReading reading) =>
        reading.Planets
            .Where(p => (p.Touches || p.IsLord) && p.Lean == InfluenceLean.Supports)
            .OrderBy(p => FunctionalRank(p.Functional))
            .ThenBy(p => p.IsNaturalMalefic)
            .Take(Named)
            .ToList();

    /// <summary>Planets that act on the target or rule it and lean against it (a dusthāna from it,
    /// its bādhaka, or in the bādhaka sthāna) — the bādhaka first, then functional malefics.</summary>
    public static IReadOnlyList<PlanetInfluence> Hinderers(TargetInfluenceReading reading) =>
        reading.Planets
            .Where(p => (p.Touches || p.IsLord || p.IsBaadhaka) && p.Lean is InfluenceLean.Obstructs or InfluenceLean.Mixed)
            .OrderByDescending(p => p.IsBaadhaka)
            .ThenBy(p => -FunctionalRank(p.Functional))
            .Take(Named)
            .ToList();

    /// <summary>Why a planet is named: its role, how it reaches the target, where it stands from it.</summary>
    public static string Why(PlanetInfluence p)
    {
        var parts = new List<string>();
        if (p.IsLord) parts.Add("its ruler");
        if (p.IsBaadhaka) parts.Add("its bādhaka");
        parts.AddRange(Links(p.Links));
        if (p.Links.HasFlag(InfluenceLink.Occupies) is false) parts.Add(PositionText(p.HouseFromTarget, p.Position));
        if (p.InBaadhakaSthaana) parts.Add("in its bādhaka sign");
        return string.Join(", ", parts);
    }

    public static IEnumerable<string> Links(InfluenceLink links)
    {
        if (links.HasFlag(InfluenceLink.Occupies)) yield return "sits in it";
        if (links.HasFlag(InfluenceLink.GrahaDrishti)) yield return "aspects it";
        if (links.HasFlag(InfluenceLink.RasiDrishti)) yield return "sign aspect";
        if (links.HasFlag(InfluenceLink.Argala)) yield return "intervenes (Argala)";
        if (links.HasFlag(InfluenceLink.Virodhargala)) yield return "blocks an intervention";
    }

    /// <summary>"8th from it (obstacles)", "a quadrant from it (sustains)" — PVR step 5's words.</summary>
    public static string PositionText(int house, TargetPosition position)
    {
        var effects = new List<string>();
        if (position.HasFlag(TargetPosition.Quadrant)) effects.Add("sustains");
        if (position.HasFlag(TargetPosition.Trine)) effects.Add("prospers");
        if (position.HasFlag(TargetPosition.Upachaya)) effects.Add("grows");
        if (position.HasFlag(TargetPosition.Dusthana)) effects.Add("obstacles");
        return $"{Ordinal(house)} from it" + (effects.Count == 0 ? "" : $" ({string.Join(", ", effects)})");
    }

    public static string FunctionalText(FunctionalNature? nature) => nature switch
    {
        FunctionalNature.Yogakaraka => "Yogakāraka",
        FunctionalNature.Benefic => "Benefic",
        FunctionalNature.Malefic => "Malefic",
        FunctionalNature.Neutral => "Neutral",
        _ => "—",
    };

    public static string LeanText(InfluenceLean lean) => lean switch
    {
        InfluenceLean.Supports => "Helps",
        InfluenceLean.Obstructs => "Hinders",
        InfluenceLean.Mixed => "Mixed",
        _ => "—",
    };

    private static int FunctionalRank(FunctionalNature? nature) => nature switch
    {
        FunctionalNature.Yogakaraka => 0,
        FunctionalNature.Benefic => 1,
        FunctionalNature.Neutral => 2,
        null => 3,
        _ => 4,
    };

    public static string Ordinal(int n) =>
        n + (n % 100 is 11 or 12 or 13 ? "th" : (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" });
}
