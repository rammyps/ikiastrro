using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;

namespace Ikiastrro.Core.Engines.Transits;

public sealed record GocharaVedhaRule(
    PlanetName TransitPlanet,
    int AuspiciousHouse,
    int VedhaHouse,
    PlanetName? ExcludedObstructorPlanet = null);

public sealed record GocharaVedhaResult(
    PlanetName TransitPlanet,
    int TransitHouseFromNatalMoon,
    bool IsAuspicious,
    int? VedhaHouse,
    IReadOnlyList<PlanetName> Obstructors)
{
    public bool IsObstructed => Obstructors.Count > 0;
    public bool CanDeliverAuspiciousResult => IsAuspicious && !IsObstructed;
}

/// <summary>PVR ch.26.3 rāśi-gochara Vedha, counted from the natal Moon.</summary>
public static class GocharaVedhaCalculator
{
    public static GocharaVedhaResult Evaluate(
        PlanetName transitPlanet,
        ZodiacName natalMoonSign,
        IReadOnlyDictionary<PlanetName, ZodiacName> transitSigns,
        IReadOnlyList<GocharaVedhaRule> rules)
    {
        if (!transitSigns.TryGetValue(transitPlanet, out var transitSign))
            throw new ArgumentException($"No transit sign supplied for {transitPlanet}.", nameof(transitSigns));

        var house = ((Array.IndexOf(HouseEngine.SignOrder, transitSign)
                      - Array.IndexOf(HouseEngine.SignOrder, natalMoonSign) + 12) % 12) + 1;
        var rule = rules.SingleOrDefault(r => r.TransitPlanet == transitPlanet && r.AuspiciousHouse == house);
        if (rule is null)
            return new GocharaVedhaResult(transitPlanet, house, false, null, Array.Empty<PlanetName>());

        var vedhaSign = SignAtHouse(natalMoonSign, rule.VedhaHouse);
        var obstructors = transitSigns
            .Where(kv => kv.Key != transitPlanet
                         && kv.Key != rule.ExcludedObstructorPlanet
                         && kv.Value == vedhaSign)
            .Select(kv => kv.Key)
            .OrderBy(p => (int)p)
            .ToList();

        return new GocharaVedhaResult(transitPlanet, house, true, rule.VedhaHouse, obstructors);
    }

    private static ZodiacName SignAtHouse(ZodiacName first, int house) =>
        HouseEngine.SignOrder[(Array.IndexOf(HouseEngine.SignOrder, first) + house - 1) % 12];
}
