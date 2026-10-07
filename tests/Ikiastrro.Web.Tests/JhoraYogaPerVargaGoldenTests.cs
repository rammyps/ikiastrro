using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Yoga;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Web.Tests;

/// <summary>ikiastrro's per-varga yoga confirmation layer (<see cref="ProductionYogaEngine.DetectForVarga"/>)
/// against JHora's Yogas list for the same chart, one list per divisional chart
/// (cproj_win_app_explorer/explorer_output/jhora/yogas-per-varga/yogas_per_varga.json, captured 2026-10-07 from
/// a running JHora holding RamakrishnanP — Chennai, India, Aries lagna). Varga positions are ikiastrro's own
/// (they match JHora's per decisions 005/007/008).</summary>
public sealed class JhoraYogaPerVargaGoldenTests
{
    private static readonly Dictionary<string, (string Planet, string Sign, int House, double Lon)[]> Vargas = new()
    {
        ["D1"] = [("Sun","Aries",1,8.200907), ("Moon","Scorpio",8,217.207773), ("Mars","Aries",1,3.942347), ("Mercury","Aries",1,1.831613), ("Jupiter","Virgo",6,158.722895), ("Venus","Aries",1,11.982233), ("Saturn","Virgo",6,160.958552), ("Rahu","Cancer",4,102.918962), ("Ketu","Capricornus",10,282.918962)],
        ["D12"] = [("Sun","Cancer",4,98.410886), ("Moon","Capricornus",10,86.493278), ("Mars","Taurus",2,47.308166), ("Mercury","Aries",1,21.979354), ("Jupiter","Sagittarius",9,104.67474), ("Venus","Leo",5,143.786794), ("Saturn","Capricornus",10,131.502619), ("Rahu","Sagittarius",9,155.027545), ("Ketu","Gemini",3,155.027545)],
        ["D2"] = [("Sun","Aries",1,16.401814), ("Moon","Cancer",4,74.415546), ("Mars","Aries",1,7.884694), ("Mercury","Aries",1,3.663226), ("Jupiter","Pisces",12,317.44579), ("Venus","Aries",1,23.964466), ("Saturn","Pisces",12,321.917103), ("Rahu","Scorpio",8,205.837924), ("Ketu","Scorpio",8,205.837924)],
        ["D3"] = [("Sun","Aries",1,24.602722), ("Moon","Pisces",12,291.62332), ("Mars","Aries",1,11.827042), ("Mercury","Aries",1,5.494838), ("Jupiter","Virgo",6,116.168685), ("Venus","Taurus",2,35.946699), ("Saturn","Leo",5,122.875655), ("Rahu","Aquarius",11,308.756886), ("Ketu","Leo",5,128.756886)],
        ["D30"] = [("Sun","Aquarius",11,246.027216), ("Moon","Virgo",6,36.233196), ("Mars","Aries",1,118.270415), ("Mercury","Aries",1,54.948384), ("Jupiter","Virgo",6,81.686851), ("Venus","Sagittarius",9,359.466986), ("Saturn","Virgo",6,148.756548), ("Rahu","Pisces",12,207.568862), ("Ketu","Pisces",12,207.568862)],
        ["D9"] = [("Sun","Gemini",3,73.808165), ("Moon","Virgo",6,154.869959), ("Mars","Taurus",2,35.481125), ("Mercury","Aries",1,16.484515), ("Jupiter","Pisces",12,348.506055), ("Venus","Cancer",4,107.840096), ("Saturn","Aries",1,8.626964), ("Rahu","Libra",7,206.270659), ("Ketu","Aries",1,26.270659)],
    };

    internal static ChartBundle Bundle()
    {
        var charts = Vargas.Select(kv => new ChartAnalysisInput(kv.Key, ZodiacName.Aries,
            kv.Value.Select(p => new PlanetPosition { Planet = p.Planet, Sign = p.Sign, HouseNumber = p.House,
                NirayanaLongitudeDegrees = p.Lon, VargaLongitudeDegrees = p.Lon }).ToList())).ToList();
        var positions = new SiderealPositions(10, new Dictionary<PlanetName, double>(),
            new Dictionary<PlanetName, double>(), new Dictionary<PlanetName, double>(), 0, 0);
        var sun = new SunTimes(DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch.AddHours(12),
            DateTimeOffset.UnixEpoch.AddDays(1), false);
        return new(new BirthDetails { Sex = "Male" }, positions, sun, charts, new Dictionary<string, string>(), []);
    }

    // Yogas on which JHora and ikiastrro share a definition. Divergences are deliberately outside this set
    // (see docs/cli/yoga-per-varga-design.md): Adhi (JHora needs >=2 benefics), Subha (JHora = kartari only),
    // Mridanga (JHora: lagna-lord strength is D1-based), Chaamara (JHora counts benefics across 7/9/10),
    // Vesi/Vosi (JHora treats them as exclusive of Ubhayachara), Parvata, Kaahala, Vimala (no PVR variant here).
    private static readonly HashSet<string> Shared =
    [
        "YOGA_RUCHAKA", "YOGA_SASA", "YOGA_SUNAPHA", "YOGA_ANAPHA", "YOGA_GAJAKESARI", "YOGA_SARPA", "YOGA_SULA",
        "YOGA_PASA", "YOGA_KEDARA", "YOGA_DAMNI", "YOGA_DHARMA_KARMADHIPATI", "YOGA_BUDHA_ADITYA", "YOGA_UBHAYACHARI",
    ];

    // JHora's lists (captured 2026-10-07), reduced to the shared yogas.
    public static IEnumerable<object[]> Golden() =>
    [
        ["D1", new[] { "YOGA_RUCHAKA", "YOGA_BUDHA_ADITYA", "YOGA_SARPA", "YOGA_SULA", "YOGA_DHARMA_KARMADHIPATI" }],
        ["D2", new[] { "YOGA_RUCHAKA", "YOGA_BUDHA_ADITYA", "YOGA_SULA", "YOGA_DHARMA_KARMADHIPATI" }],
        ["D3", new[] { "YOGA_RUCHAKA", "YOGA_BUDHA_ADITYA", "YOGA_SUNAPHA", "YOGA_GAJAKESARI", "YOGA_PASA" }],
        ["D9", new[] { "YOGA_UBHAYACHARI", "YOGA_GAJAKESARI", "YOGA_DAMNI" }],
        ["D12", new[] { "YOGA_SASA", "YOGA_ANAPHA", "YOGA_DAMNI" }],
        ["D30", new[] { "YOGA_RUCHAKA", "YOGA_GAJAKESARI", "YOGA_KEDARA", "YOGA_DHARMA_KARMADHIPATI" }],
    ];

    [Theory, MemberData(nameof(Golden))]
    public void SharedYogasMatchJhoraInEachVarga(string varga, string[] jhora)
    {
        var present = Present(varga);
        Assert.Equal(jhora.OrderBy(x => x), present.Where(Shared.Contains).Distinct().OrderBy(x => x));
    }

    [Fact]
    public void SingleChartYogasOnVargaReferenceMatchHowJhoraReadsThem()
    {
        // JHora lists Vosi in D2 (Jupiter, Saturn in 12th from Sun) and Vesi in D3 and D12.
        Assert.Contains("YOGA_VASI", Present("D2"));
        Assert.Contains("YOGA_VESI", Present("D3"));
        Assert.Contains("YOGA_VESI", Present("D12"));
    }

    [Fact]
    public void OnlyTheChartsJhoraRulesNameAreYogaReferenceCharts()
    {
        Assert.Equal(["D1", "D2", "D3", "D9", "D12", "D30"], ProductionYogaEngine.VargaReferenceCharts);
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProductionYogaEngine().DetectForVarga(Bundle(), "D10"));
    }

    [Fact]
    public void D1ReferenceRowsAreAlsoPartOfTheFullD1Evaluation()
    {
        // The varga layer must never invent a D1 result: every D1 row it yields exists, identically, in DetectDetailed.
        var engine = new ProductionYogaEngine();
        var full = engine.DetectDetailed(Bundle()).ToDictionary(x => (x.Result.SourceRefCode, x.Result.SourceVariantCode));
        foreach (var row in engine.DetectForVarga(Bundle(), "D1"))
        {
            var key = (row.Result.SourceRefCode, row.Result.SourceVariantCode);
            Assert.True(full.ContainsKey(key), $"{key} missing from DetectDetailed");
            Assert.Equal(full[key].Result.Present, row.Result.Present);
        }
    }

    // Where PVR defines a yoga, its variant is JHora's wording, so it decides; otherwise any source does.
    private static IReadOnlyList<string> Present(string varga)
    {
        var rows = new ProductionYogaEngine().DetectForVarga(Bundle(), varga).Where(x => x.Result.Present == true).ToList();
        var pvr = rows.Where(x => x.Result.SourceRefCode == "SRC_PVR_INTEGRATED").Select(x => x.Result.YogaCode).ToHashSet();
        return rows.Where(x => x.Result.SourceRefCode == "SRC_PVR_INTEGRATED" || !pvr.Contains(x.Result.YogaCode))
            .Select(x => x.Result.YogaCode).Distinct().ToList();
    }
}
