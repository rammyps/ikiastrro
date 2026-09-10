using Ikiastrro.Core.Engines.Ashtakavarga;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

public class AshtakavargaCalculatorTests
{
    private static ChartAnalysisInput D1(ZodiacName lagna, params (string Planet, ZodiacName Sign)[] grahas)
    {
        var planets = grahas
            .Select(g => new PlanetPosition { Planet = g.Planet, PointKind = "Graha", Sign = g.Sign.ToString() })
            .ToList();
        return new ChartAnalysisInput("D1", lagna, planets);
    }

    /// <summary>The Jagannatha Hora export for 1_Ramakrishnan: natal D1 sign of every graha + Lagna.</summary>
    private static ChartAnalysisInput RamakrishnanD1() => D1(
        ZodiacName.Aries,
        ("Sun", ZodiacName.Aries), ("Moon", ZodiacName.Scorpio), ("Mars", ZodiacName.Aries),
        ("Mercury", ZodiacName.Aries), ("Jupiter", ZodiacName.Virgo), ("Venus", ZodiacName.Aries),
        ("Saturn", ZodiacName.Virgo));

    [Fact]
    public void Sav_grand_total_is_always_337()
    {
        // Arbitrary natal layout — the SAV grand total is a chart invariant.
        var input = D1(
            ZodiacName.Cancer,
            ("Sun", ZodiacName.Leo), ("Moon", ZodiacName.Aquarius), ("Mars", ZodiacName.Gemini),
            ("Mercury", ZodiacName.Cancer), ("Jupiter", ZodiacName.Sagittarius), ("Venus", ZodiacName.Taurus),
            ("Saturn", ZodiacName.Libra));

        Assert.Equal(337, AshtakavargaCalculator.Calculate(input).Sarva.Total);
    }

    [Fact]
    public void Ramakrishnan_bhinna_ashtakavarga_matches_jhora()
    {
        var av = AshtakavargaCalculator.Calculate(RamakrishnanD1());

        int[] Row(string r) => av.Bhinna.Single(b => b.Recipient == r).Bindus.ToArray();

        Assert.Equal(new[] { 4, 4, 3, 5, 2, 5, 4, 2, 4, 6, 5, 4 }, Row("SUN"));
        Assert.Equal(new[] { 3, 2, 6, 5, 4, 5, 4, 4, 2, 7, 6, 1 }, Row("MOON"));
        Assert.Equal(new[] { 4, 2, 5, 3, 3, 6, 1, 2, 1, 4, 6, 2 }, Row("MARS"));
        Assert.Equal(new[] { 7, 4, 4, 5, 5, 5, 2, 3, 6, 3, 7, 3 }, Row("MERCURY"));
        Assert.Equal(new[] { 5, 6, 2, 6, 4, 5, 4, 4, 6, 6, 6, 2 }, Row("JUPITER"));
        Assert.Equal(new[] { 4, 4, 7, 6, 3, 3, 1, 5, 6, 4, 6, 3 }, Row("VENUS"));
        Assert.Equal(new[] { 3, 1, 2, 4, 2, 5, 1, 3, 1, 7, 7, 3 }, Row("SATURN"));
    }

    [Fact]
    public void Ramakrishnan_sarva_ashtakavarga_matches_jhora()
    {
        var av = AshtakavargaCalculator.Calculate(RamakrishnanD1());
        Assert.Equal(new[] { 30, 23, 29, 34, 23, 34, 17, 23, 26, 37, 43, 18 }, av.Sarva.Bindus.ToArray());
        Assert.Equal(337, av.Sarva.Total);
    }

    [Fact]
    public void Ramakrishnan_sodhya_pinda_matches_jhora_exactly()
    {
        var av = AshtakavargaCalculator.Calculate(RamakrishnanD1());
        (int Rasi, int Graha, int Sodhya) P(string r)
        {
            var p = av.Pinda.Single(x => x.Recipient == r);
            return (p.RasiPinda, p.GrahaPinda, p.SodhyaPinda);
        }

        Assert.Equal((38, 65, 103), P("SUN"));
        Assert.Equal((114, 85, 199), P("MOON"));
        Assert.Equal((97, 135, 232), P("MARS"));
        Assert.Equal((106, 80, 186), P("MERCURY"));
        Assert.Equal((90, 35, 125), P("JUPITER"));
        Assert.Equal((136, 35, 171), P("VENUS"));
        Assert.Equal((48, 110, 158), P("SATURN"));
    }

    [Fact]
    public void TrikonaSodhana_rules()
    {
        // Trine {0,4,8}: a zero anywhere -> no reduction.
        Assert.Equal(new[] { 3, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0 },
            AshtakavargaCalculator.TrikonaSodhana(new[] { 3, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0 }));

        // All three equal (and non-zero) -> all zero.
        Assert.Equal(new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            AshtakavargaCalculator.TrikonaSodhana(new[] { 4, 0, 0, 0, 4, 0, 0, 0, 4, 0, 0, 0 }));

        // Otherwise subtract the lowest of the three.
        Assert.Equal(new[] { 1, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0 },
            AshtakavargaCalculator.TrikonaSodhana(new[] { 3, 0, 0, 0, 5, 0, 0, 0, 2, 0, 0, 0 }));
    }

    [Fact]
    public void EkadhipatyaSodhana_rules()
    {
        var none = new HashSet<int>();
        // Pair (Ta=1, Li=6), both empty, unequal -> both take the lower value.
        var bothEmptyUnequal = new int[12]; bothEmptyUnequal[1] = 5; bothEmptyUnequal[6] = 2;
        var r1 = AshtakavargaCalculator.EkadhipatyaSodhana(bothEmptyUnequal, none);
        Assert.Equal(2, r1[1]);
        Assert.Equal(2, r1[6]);

        // Both empty, equal -> both zero.
        var bothEmptyEqual = new int[12]; bothEmptyEqual[1] = 4; bothEmptyEqual[6] = 4;
        var r2 = AshtakavargaCalculator.EkadhipatyaSodhana(bothEmptyEqual, none);
        Assert.Equal(0, r2[1]);
        Assert.Equal(0, r2[6]);

        // One occupied (Ta), the other empty (Li): empty <= occupied -> empty = 0.
        var oneOcc = new int[12]; oneOcc[1] = 4; oneOcc[6] = 4;
        var r3 = AshtakavargaCalculator.EkadhipatyaSodhana(oneOcc, new HashSet<int> { 1 });
        Assert.Equal(4, r3[1]);
        Assert.Equal(0, r3[6]);

        // One occupied (Ta), empty (Li) greater -> empty = occupied's value.
        var oneOccGreater = new int[12]; oneOccGreater[1] = 2; oneOccGreater[6] = 6;
        var r4 = AshtakavargaCalculator.EkadhipatyaSodhana(oneOccGreater, new HashSet<int> { 1 });
        Assert.Equal(2, r4[1]);
        Assert.Equal(2, r4[6]);
    }
}
