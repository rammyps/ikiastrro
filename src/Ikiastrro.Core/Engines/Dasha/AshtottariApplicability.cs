using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Karakas;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Dasha;

/// <summary>One of the three views on when Ashtottari dasa applies, and whether this chart meets it.</summary>
public sealed record AshtottariView(string Name, bool Holds, string Detail);

/// <summary>
/// When Ashtottari dasa applies, P.V.R. Narasimha Rao, <i>Vedic Astrology: An Integrated Approach</i> sec.17.1 and 17.2.3,
/// printed pp.226-229 (<c>SRC_PVR_INTEGRATED</c>). The book says the conditions are highly controversial and gives three
/// views: (1) every chart; (2) Rahu, not in the Lagna, in a quadrant or a trine from the Lagna lord; (3) daytime births in
/// Krishna paksha and night-time births in Shukla paksha. All three are reported and none is chosen. Pure; no I/O.
/// </summary>
public static class AshtottariApplicability
{
    public static IReadOnlyList<AshtottariView> Evaluate(ChartAnalysisInput chart, bool isNightBirth, bool krishnaPaksha)
    {
        var signOf = chart.Planets
            .Where(p => p.Planet != "Ascendant" && Enum.TryParse<PlanetName>(p.Planet, out _))
            .ToDictionary(p => Enum.Parse<PlanetName>(p.Planet), p => Enum.Parse<ZodiacName>(p.Sign));

        var views = new List<AshtottariView> { new("1. Applicable in all charts", true, "Under this view it always applies.") };

        if (signOf.Count == 9)
        {
            var lagna = chart.AscendantSign;
            var lord = StrongerCoLord.For(lagna, chart);
            var rahu = signOf[PlanetName.Rahu];
            var house = ((int)rahu - (int)signOf[lord] + 12) % 12 + 1;
            var inLagna = rahu == lagna;
            var holds = !inLagna && house is 1 or 4 or 7 or 10 or 5 or 9;
            views.Add(new("2. Rahu, not in the Lagna, in a quadrant or trine from the Lagna lord", holds,
                $"Rahu is in {Label(rahu)}{(inLagna ? ", the Lagna" : "")}; the Lagna lord {lord} is in {Label(signOf[lord])}, so Rahu is the {Ordinal(house)} from it."));
        }

        var paksha = krishnaPaksha ? "Krishna" : "Shukla";
        var time = isNightBirth ? "night" : "day";
        views.Add(new("3. Daytime birth in Krishna paksha, or night-time birth in Shukla paksha", isNightBirth != krishnaPaksha,
            $"A {time}-time birth in {paksha} paksha."));
        return views;
    }

    private static string Label(ZodiacName s) => s == ZodiacName.Capricornus ? "Capricorn" : s.ToString();
    private static string Ordinal(int n) => n switch { 1 => "1st", 2 => "2nd", 3 => "3rd", _ => $"{n}th" };
}
