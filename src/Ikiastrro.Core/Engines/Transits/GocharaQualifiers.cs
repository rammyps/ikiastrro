using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Strength;

namespace Ikiastrro.Core.Engines.Transits;

/// <summary>The natal D1 facts a transit is qualified against. Whole-sign houses from the Lagna.
/// Strengths are pre-banded by the caller with <see cref="StrengthBands"/> (planets by Ṣaḍbala % of
/// minimum, houses by independent Bhava Bala z-score), so Core never reads the database.</summary>
/// <param name="HouseLords">House 1–12 → the lord of its sign.</param>
/// <param name="NatalSigns">Each graha's natal sign, Rāhu and Ketu included.</param>
public sealed record GocharaNatalContext(
    ZodiacName Ascendant,
    IReadOnlyDictionary<int, PlanetName> HouseLords,
    IReadOnlyDictionary<PlanetName, ZodiacName> NatalSigns,
    IReadOnlyDictionary<PlanetName, StrengthTier> PlanetStrength,
    IReadOnlyDictionary<int, StrengthTier> HouseStrength);

/// <summary>The Vimśottarī lords running on the chosen date; null where the level is not selected.</summary>
public sealed record GocharaDashaLords(PlanetName? Maha, PlanetName? Antar, PlanetName? Pratyantar)
{
    public IEnumerable<(string Level, PlanetName Lord)> Running()
    {
        if (Maha is { } m) yield return ("Mahādaśā", m);
        if (Antar is { } a) yield return ("Antardaśā", a);
        if (Pratyantar is { } p) yield return ("Pratyantardaśā", p);
    }
}

/// <summary>Whether the transited natal house is backed by its own lord and its own house strength:
/// Promised (neither weak), Weak (both weak), Mixed otherwise; Unknown when neither strength is on
/// file. A project heuristic (docs/architecture/transit_gochara_inference.md §9).</summary>
public enum GocharaPromise { Unknown, Promised, Mixed, Weak }

public enum DashaLinkKind { TransitPlanetIsLord, LordRulesHouse, LordOccupiesSign }

/// <param name="Level">Mahādaśā, Antardaśā or Pratyantardaśā.</param>
public sealed record DashaLink(string Level, PlanetName Lord, DashaLinkKind Kind);

/// <param name="HouseFromLagna">The natal house whose sign the planet is crossing.</param>
/// <param name="PlanetLordships">Houses the transit planet rules natally (empty for Rāhu and Ketu).</param>
/// <param name="Foreground">True when the transit is linked to a running dasha lord; false means it is
/// background to the dasha, not unimportant.</param>
public sealed record GocharaPlanetQualifier(
    PlanetName Planet,
    ZodiacName TransitSign,
    int HouseFromLagna,
    PlanetName HouseLord,
    IReadOnlyList<PlanetName> NatalOccupants,
    IReadOnlyList<int> PlanetLordships,
    int? PlanetNatalHouse,
    StrengthTier PlanetStrength,
    StrengthTier HouseStrength,
    StrengthTier HouseLordStrength,
    GocharaPromise Promise,
    IReadOnlyList<DashaLink> DashaLinks,
    bool Foreground,
    IReadOnlyList<string> Lines);

/// <summary>
/// Qualifies each slow planet's Gochara against the natal chart and the running dasha (PVR ch.25–26,
/// Raman: a transit confirms or tempers a natal and dasha promise, it does not create one). It states
/// structural facts — which natal house is crossed, what that house is promised by its lord and its own
/// strength, and which running dasha lords the planet touches — and never an event. Pure.
/// </summary>
public static class GocharaQualifierBuilder
{
    public static GocharaPromise PromiseOf(StrengthTier lord, StrengthTier house)
    {
        var known = new[] { lord, house }.Where(t => t != StrengthTier.None).ToList();
        if (known.Count == 0) return GocharaPromise.Unknown;
        var weak = known.Count(t => t == StrengthTier.Weak);
        return weak == 0 ? GocharaPromise.Promised
            : weak == known.Count ? GocharaPromise.Weak
            : GocharaPromise.Mixed;
    }

    public static GocharaPlanetQualifier Qualify(
        PlanetName planet,
        ZodiacName transitSign,
        GocharaNatalContext natal,
        GocharaDashaLords dasha)
    {
        var house = AstroMath.CountFromSignToSign(natal.Ascendant, transitSign);
        var lord = natal.HouseLords[house];
        var occupants = natal.NatalSigns.Where(kv => kv.Value == transitSign && kv.Key != planet)
            .Select(kv => kv.Key).OrderBy(p => p).ToList();
        var lordships = natal.HouseLords.Where(kv => kv.Value == planet).Select(kv => kv.Key).OrderBy(h => h).ToList();
        int? natalHouse = natal.NatalSigns.TryGetValue(planet, out var natalSign)
            ? AstroMath.CountFromSignToSign(natal.Ascendant, natalSign) : null;
        var planetStrength = natal.PlanetStrength.GetValueOrDefault(planet, StrengthTier.None);
        var houseStrength = natal.HouseStrength.GetValueOrDefault(house, StrengthTier.None);
        var lordStrength = natal.PlanetStrength.GetValueOrDefault(lord, StrengthTier.None);
        var promise = PromiseOf(lordStrength, houseStrength);

        var links = new List<DashaLink>();
        foreach (var (level, dashaLord) in dasha.Running())
        {
            if (dashaLord == planet) links.Add(new DashaLink(level, dashaLord, DashaLinkKind.TransitPlanetIsLord));
            if (dashaLord == lord) links.Add(new DashaLink(level, dashaLord, DashaLinkKind.LordRulesHouse));
            if (natal.NatalSigns.TryGetValue(dashaLord, out var dashaSign) && dashaSign == transitSign && dashaLord != planet)
                links.Add(new DashaLink(level, dashaLord, DashaLinkKind.LordOccupiesSign));
        }

        return new GocharaPlanetQualifier(planet, transitSign, house, lord, occupants, lordships, natalHouse,
            planetStrength, houseStrength, lordStrength, promise, links, links.Count > 0,
            Lines(planet, transitSign, house, lord, occupants, lordships, natalHouse, planetStrength,
                houseStrength, lordStrength, promise, links, dasha));
    }

    private static List<string> Lines(
        PlanetName planet, ZodiacName sign, int house, PlanetName lord, IReadOnlyList<PlanetName> occupants,
        IReadOnlyList<int> lordships, int? natalHouse, StrengthTier planetStrength, StrengthTier houseStrength,
        StrengthTier lordStrength, GocharaPromise promise, IReadOnlyList<DashaLink> links, GocharaDashaLords dasha)
    {
        var lines = new List<string>();

        var crossing = $"Crosses your natal {Ordinal(house)} house ({Disp(sign)}), ruled by {lord}";
        if (occupants.Count > 0) crossing += $", where natal {string.Join(" and ", occupants)} sit{(occupants.Count == 1 ? "s" : "")}";
        lines.Add(crossing + ".");

        lines.Add($"Natal promise of that house: {PromiseWord(promise)} — its lord {lord} has {Band(lordStrength)} Ṣaḍbala, " +
                  $"the house has {Band(houseStrength)} Bhava Bala (independent of the lord).");

        var role = lordships.Count > 0
            ? $"{planet} rules your {JoinOrdinals(lordships)}"
            : $"{planet} rules no house";
        role += natalHouse is { } nh ? $" and sits in the {Ordinal(nh)}" : string.Empty;
        lines.Add($"{role}; its own Ṣaḍbala is {Band(planetStrength)}.");

        if (links.Count > 0)
            lines.AddRange(links.Select(Describe));
        else if (dasha.Running().Any())
            lines.Add($"No link to the running dasha ({string.Join(" / ", dasha.Running().Select(r => r.Lord))}): it is background to the dasha.");
        else
            lines.Add("No dasha selected, so no dasha link is read.");

        return lines;
    }

    private static string Describe(DashaLink l) => l.Kind switch
    {
        DashaLinkKind.TransitPlanetIsLord => $"{l.Level} lord is {l.Lord} itself — the transit is the running period's own planet.",
        DashaLinkKind.LordRulesHouse => $"{l.Level} lord {l.Lord} rules the house being crossed.",
        _ => $"{l.Level} lord {l.Lord} sits natally in the sign being crossed.",
    };

    private static string PromiseWord(GocharaPromise p) => p switch
    {
        GocharaPromise.Promised => "promised",
        GocharaPromise.Weak => "weakly promised",
        GocharaPromise.Mixed => "mixed",
        _ => "not on file",
    };

    private static string Band(StrengthTier t) => t switch
    {
        StrengthTier.Strong => "strong",
        StrengthTier.Moderate => "moderate",
        StrengthTier.Weak => "weak",
        _ => "unrecorded",
    };

    private static string JoinOrdinals(IReadOnlyList<int> houses) =>
        string.Join(houses.Count == 2 ? " and " : ", ", houses.Select(Ordinal));

    private static string Ordinal(int n) =>
        n + (n % 100 is 11 or 12 or 13 ? "th" : (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" });

    private static string Disp(ZodiacName s) => s == ZodiacName.Capricornus ? "Capricorn" : s.ToString();
}
