using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Yoga.Tests;

/// <summary>JHora's Vaiseshikamsa table (Edit ▸ Copy complete calculations, RamakrishnanP.jhd,
/// explorer_output/jhora/clipboard) against <see cref="JhoraVaiseshikamsa"/>, fed JHora's own varga signs
/// for the seven classical grahas (Sun, Moon, Mars, Mercury, Jupiter, Venus, Saturn).</summary>
public sealed class JhoraVaiseshikamsaGoldenTests
{
    private static readonly PlanetName[] Grahas =
        [PlanetName.Sun, PlanetName.Moon, PlanetName.Mars, PlanetName.Mercury, PlanetName.Jupiter, PlanetName.Venus, PlanetName.Saturn];

    private static readonly Dictionary<string, string[]> VargaSigns = new()
    {
        ["D1"] = ["Aries", "Scorpio", "Aries", "Aries", "Virgo", "Aries", "Virgo"],
        ["D2"] = ["Aries", "Cancer", "Aries", "Aries", "Pisces", "Aries", "Pisces"],
        ["D3"] = ["Aries", "Pisces", "Aries", "Aries", "Virgo", "Taurus", "Leo"],
        ["D4"] = ["Cancer", "Scorpio", "Aries", "Aries", "Sagittarius", "Cancer", "Sagittarius"],
        ["D7"] = ["Taurus", "Aries", "Aries", "Aries", "Capricornus", "Gemini", "Capricornus"],
        ["D9"] = ["Gemini", "Leo", "Taurus", "Aries", "Pisces", "Cancer", "Aries"],
        ["D10"] = ["Gemini", "Aquarius", "Taurus", "Aries", "Scorpio", "Cancer", "Libra"],
        ["D12"] = ["Cancer", "Sagittarius", "Taurus", "Aries", "Sagittarius", "Leo", "Capricornus"],
        ["D16"] = ["Leo", "Virgo", "Gemini", "Aries", "Scorpio", "Libra", "Libra"],
        ["D20"] = ["Virgo", "Pisces", "Gemini", "Aries", "Capricornus", "Scorpio", "Pisces"],
        ["D24"] = ["Aquarius", "Aries", "Scorpio", "Virgo", "Capricornus", "Taurus", "Scorpio"],
        ["D27"] = ["Scorpio", "Taurus", "Cancer", "Taurus", "Aquarius", "Aquarius", "Aries"],
        ["D30"] = ["Aquarius", "Taurus", "Aries", "Aries", "Virgo", "Sagittarius", "Virgo"],
        ["D40"] = ["Aquarius", "Aries", "Virgo", "Taurus", "Virgo", "Cancer", "Sagittarius"],
        ["D45"] = ["Aries", "Pisces", "Virgo", "Gemini", "Capricornus", "Virgo", "Aries"],
        ["D60"] = ["Leo", "Gemini", "Scorpio", "Gemini", "Libra", "Pisces", "Gemini"],
    };

    // JHora: Dasa Varga (10) / Shodasa Varga (16); null = blank (fewer than 2).
    private static readonly (string? Dasa, string? Shodasa)[] Golden =
    [
        ("5-Simhasana", "6-Kerala"),
        ("2-Paarijaata", "3-Kusuma"),
        ("6-Paaraavata", "8-ChandanaVana"),
        (null, "3-Kusuma"),
        ("3-Uttama", "4-Nagapurusha"),
        ("3-Uttama", "4-Nagapurusha"),
        ("4-Gopura", "4-Nagapurusha"),
    ];

    private static IReadOnlyList<ChartAnalysisInput> Charts() =>
        VargaSigns.Select(kv => new ChartAnalysisInput(kv.Key, ZodiacName.Aries,
            Grahas.Select((g, i) => new PlanetPosition { Planet = g.ToString(), Sign = kv.Value[i], HouseNumber = 1 }).ToList())).ToArray();

    [Fact]
    public void TiersMatchJhoraForAllSevenGrahas()
    {
        var charts = Charts();
        for (var i = 0; i < Grahas.Length; i++)
        {
            var dasa = JhoraVaiseshikamsa.DasaTier(JhoraVaiseshikamsa.Count(charts, Grahas[i], JhoraVaiseshikamsa.DasaVarga));
            var shodasa = JhoraVaiseshikamsa.ShodasaTier(JhoraVaiseshikamsa.Count(charts, Grahas[i], JhoraVaiseshikamsa.ShodasaVarga));
            Assert.Equal(Golden[i].Dasa, dasa);
            Assert.Equal(Golden[i].Shodasa, shodasa);
        }
    }

    [Fact]
    public void OwnSignOnlyCountingWouldUnderstateSunMoonSaturn()
    {
        // Raman's own-sign ladder: Sun 2 of 16, JHora's own-or-exalted: 6 — the documented divergence.
        var charts = Charts();
        Assert.Equal(2, VaiseshikamsaCalculator.SwavargaCount(charts, PlanetName.Sun));
        Assert.Equal(6, JhoraVaiseshikamsa.Count(charts, PlanetName.Sun, JhoraVaiseshikamsa.ShodasaVarga));
    }
}
