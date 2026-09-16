using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Houses;

/// <summary>
/// Rasi drishti (sign aspect) — P.V.R. Narasimha Rao, Vedic Astrology: An Integrated
/// Approach, sec.10.3 "Rasi Drishti" (SRC_PVR_INTEGRATED, verified against the raw book
/// extract):
///
///   - A movable rasi aspects all fixed rasis except the one adjacent (next) to it.
///   - A fixed rasi aspects all movable rasis except the one adjacent (previous) to it.
///   - A dual rasi aspects all other dual rasis.
///   - Symmetric: rasi Y aspects rasi X whenever X aspects Y.
///
/// Every sign aspects exactly 3 others. Distinct from graha drishti (planet -> house
/// offset, <see cref="Relationships.RelationshipEngine"/>'s AspectOffsets) — this is
/// sign -> sign, independent of any chart. Mirrors <c>tbl_Rule_RasiDrishti</c>
/// (migration 107) — cited there but not read from there.
///
/// First consumer: <see cref="Karakas.StrongerRasiComparator"/> rule 2 (PVR sec.15.5.2),
/// itself feeding <see cref="Karakas.GrahaArudhaCalculator"/>.
/// </summary>
public static class RasiDrishtiCalculator
{
    public static bool Aspects(ZodiacName from, ZodiacName to)
    {
        if (from == to) return false;
        var fromModality = BaadhakaCalculator.GetModality(from);
        var toModality = BaadhakaCalculator.GetModality(to);
        return (fromModality, toModality) switch
        {
            (SignModality.Movable, SignModality.Fixed) => to != Adjacent(from, 1),
            (SignModality.Fixed, SignModality.Movable) => to != Adjacent(from, -1),
            (SignModality.Dual, SignModality.Dual) => true,
            _ => false
        };
    }

    /// <summary>The (always exactly 3) signs <paramref name="sign"/> aspects via rasi drishti.</summary>
    public static IReadOnlyList<ZodiacName> GetAspectedSigns(ZodiacName sign) =>
        Enum.GetValues<ZodiacName>().Where(other => Aspects(sign, other)).ToArray();

    private static ZodiacName Adjacent(ZodiacName sign, int offset) =>
        (ZodiacName)((((int)sign + offset) % 12 + 12) % 12);
}
