using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.LifeMatters;
using Ikiastrro.Core.Models;
using Ikiastrro.Web.Components.Charts;
using Ikiastrro.Web.Components.Workspace;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>Key Inference's About Houses "What acts on each house" (HouseVerdicts) and Vargas
/// Vimśopaka Bala (VimsopakaRead). Fixture: Aries Lagna, the TargetInfluencesTests placements.</summary>
public class KeyInferenceAdditionsTests
{
    private static readonly (PlanetName Planet, ZodiacName Sign)[] Placements =
    [
        (PlanetName.Sun, ZodiacName.Taurus), (PlanetName.Moon, ZodiacName.Taurus),
        (PlanetName.Mars, ZodiacName.Capricornus), (PlanetName.Mercury, ZodiacName.Leo),
        (PlanetName.Jupiter, ZodiacName.Scorpio), (PlanetName.Venus, ZodiacName.Sagittarius),
        (PlanetName.Saturn, ZodiacName.Cancer), (PlanetName.Rahu, ZodiacName.Aquarius),
        (PlanetName.Ketu, ZodiacName.Leo),
    ];

    private static LoadedChart Chart(string chartType, IEnumerable<(PlanetName Planet, ZodiacName Sign)> placements) =>
        new(chartType, chartType, chartType, null, "Aries", "Taurus", null,
            placements.Select(p => new ChartKeyDetail
            {
                Planet = p.Planet.ToString(),
                Sign = p.Sign.ToString(),
                PointKind = "Graha",
                VargaLongitudeDegrees = (int)p.Sign * 30 + 15,
                NirayanaLongitudeDegrees = (int)p.Sign * 30 + 15,
                HouseNumberFromLagna = AstroMath.CountFromSignToSign(ZodiacName.Aries, p.Sign),
            }).ToList(),
            [], [], [], [], [], null, null, "test");

    [Fact]
    public void House_verdicts_read_all_twelve_houses_both_ways()
    {
        var rows = HouseVerdicts.For(Chart("D1", Placements), []);
        Assert.Equal(Enumerable.Range(1, 12), rows.Select(r => r.House));

        var fourth = rows.Single(r => r.House == 4);
        Assert.Equal(ZodiacName.Cancer, fourth.Sign);
        Assert.Equal("Moon", fourth.Raman.LordPlanet);
        Assert.Equal(PlanetName.Moon, fourth.Influences.Lord);
        Assert.Equal("Venus", fourth.Influences.Baadhaka.Baadhaka);
        Assert.Equal(InfluenceLink.Occupies, fourth.Influences.Planets.Single(p => p.Planet == PlanetName.Saturn).Links);
    }

    [Fact]
    public void House_verdicts_match_the_raman_calculator()
    {
        var chart = Chart("D1", Placements);
        var raman = HouseBeneficMaleficCalculator.ComputeAll(ZodiacName.Aries, chart.Grahas);
        Assert.Equal(raman.Select(r => r.Verdict), HouseVerdicts.For(chart, []).Select(r => r.Raman.Verdict));
    }

    [Fact]
    public void House_verdicts_are_empty_when_a_graha_is_missing() =>
        Assert.Empty(HouseVerdicts.For(Chart("D1", Placements.Where(p => p.Planet != PlanetName.Ketu)), []));

    [Fact]
    public void Vimsopaka_computes_only_the_schemes_whose_charts_exist()
    {
        var ownSigns = new (PlanetName, ZodiacName)[]
        {
            (PlanetName.Sun, ZodiacName.Leo), (PlanetName.Moon, ZodiacName.Cancer), (PlanetName.Mars, ZodiacName.Aries),
            (PlanetName.Mercury, ZodiacName.Gemini), (PlanetName.Jupiter, ZodiacName.Sagittarius),
            (PlanetName.Venus, ZodiacName.Taurus), (PlanetName.Saturn, ZodiacName.Capricornus),
            (PlanetName.Rahu, ZodiacName.Aquarius), (PlanetName.Ketu, ZodiacName.Leo),
        };
        var charts = new Dictionary<string, LoadedChart> { ["D1"] = Chart("D1", ownSigns), ["D9"] = Chart("D9", ownSigns) };
        VimsopakaWeight[] weights =
        [
            new("BOTH", "D1", 10m), new("BOTH", "D9", 10m),
            new("NEEDS_D3", "D1", 10m), new("NEEDS_D3", "D3", 10m),
        ];

        var scores = VimsopakaRead.Compute(charts, weights);

        Assert.Equal(7, scores.Count); // the seven grahas; no Rahu/Ketu
        Assert.All(scores.Values, s => Assert.Equal(20m, s["BOTH"]));
        Assert.All(scores.Values, s => Assert.False(s.ContainsKey("NEEDS_D3")));
        Assert.Equal("D1 10 · D9 10", VimsopakaRead.WeightsText(weights, "BOTH"));
    }
}
