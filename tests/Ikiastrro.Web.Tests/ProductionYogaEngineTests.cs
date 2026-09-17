using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Yoga;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Web.Tests;

public sealed class ProductionYogaEngineTests
{
    [Fact]
    public void ComposesCompleteRamanLedgerAndPvrVariantsWithoutDuplicateIdentity()
    {
        var rows = new ProductionYogaEngine().DetectDetailed(Bundle());
        Assert.Equal(300, rows.Where(x => x.Result.SourceVariantCode.StartsWith("RAMAN_300_"))
            .Select(x => Entry(x.Result.SourceVariantCode)).Distinct().Count());
        Assert.Contains(rows, x => x.Result.SourceRefCode == "SRC_PVR_INTEGRATED");
        Assert.Equal(rows.Count, rows.Select(x => (x.Result.SourceRefCode, x.Result.SourceVariantCode)).Distinct().Count());
    }

    [Fact]
    public void FinalHundredMinusRajaClusterRemainExplicitlyUnevaluated()
    {
        // 245-263 (the Raja Yoga cluster) was transcribed 2026-09-17 — RamanRajaYogaEvaluator.
        // The other 81 of the 201-300 catalog are still pure NOT_EVALUATED ledger rows.
        var rows = new ProductionYogaEngine().DetectDetailed(Bundle())
            .Where(x => x.Result.SourceVariantCode.StartsWith("RAMAN_300_")
                && Entry(x.Result.SourceVariantCode) >= 201).ToList();
        Assert.Equal(100, rows.Count);
        var raja = rows.Where(x => Entry(x.Result.SourceVariantCode) is >= 245 and <= 263).ToList();
        Assert.Equal(19, raja.Count);
        Assert.All(raja, x => Assert.Equal("EVALUATED", x.Result.EvaluationStatus));
        var stillCatalogued = rows.Except(raja).ToList();
        Assert.Equal(81, stillCatalogued.Count);
        Assert.All(stillCatalogued, x => { Assert.Equal("NOT_EVALUATED", x.Result.EvaluationStatus);
            Assert.Contains("PREDICATE_NOT_IMPLEMENTED", x.MissingRequirementCodes); });
    }

    private static ChartBundle Bundle()
    {
        var planets = new[] { "Sun", "Moon", "Mars", "Mercury", "Jupiter", "Venus", "Saturn", "Rahu", "Ketu" }
            .Select((p, i) => new PlanetPosition { Planet = p, Sign = ((ZodiacName)(i % 12)).ToString(),
                HouseNumber = i % 12 + 1, NirayanaLongitudeDegrees = i * 30d + 10 }).ToList();
        var charts = new[] { new ChartAnalysisInput("D1", ZodiacName.Aries, planets),
            new ChartAnalysisInput("D9", ZodiacName.Aries, planets) };
        var positions = new SiderealPositions(10, new Dictionary<PlanetName,double>(),
            new Dictionary<PlanetName,double>(), new Dictionary<PlanetName,double>(), 0, 0);
        var sun = new SunTimes(DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch.AddHours(12),
            DateTimeOffset.UnixEpoch.AddDays(1), false);
        return new(new BirthDetails { Sex = "Male" }, positions, sun, charts,
            new Dictionary<string,string>(), []);
    }

    private static int Entry(string variant)
    {
        var digits = new string(variant[10..].TakeWhile(char.IsDigit).ToArray());
        return int.Parse(digits);
    }
}
