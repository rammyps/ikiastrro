using Ikiastrro.Core.Models;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dispositors;
using Ikiastrro.Core.Pipeline;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

public class DispositorEngineTests
{
    private readonly DispositorEngine _engine = new();

    [Fact]
    public void ResolvesFinalSelfDispositorAcrossMultipleHops()
    {
        var rows = _engine.Compute(Chart(
            P("Moon", "Aries"), P("Mars", "Taurus"), P("Venus", "Libra")));
        var moon = rows.Single(r => r.Planet == "Moon");
        Assert.Equal(new[] { "Moon", "Mars", "Venus", "Venus" }, moon.Chain);
        Assert.Equal("Venus", moon.FinalDispositor);
        Assert.Equal("SELF_DISPOSED", moon.TerminationCode);
    }

    [Fact]
    public void DetectsMutualReceptionWithoutInventingFinalDispositor()
    {
        var rows = _engine.Compute(Chart(P("Mars", "Taurus"), P("Venus", "Aries")));
        var mars = rows.Single(r => r.Planet == "Mars");
        Assert.True(mars.InMutualReception);
        Assert.Null(mars.FinalDispositor);
        Assert.Equal(new[] { "Mars", "Venus" }, mars.Cycle);
    }

    [Fact]
    public void StopsSafelyWhenARequiredLordPlacementIsMissing()
    {
        var moon = _engine.Compute(Chart(P("Moon", "Aries"))).Single();
        Assert.Equal("MISSING_PLACEMENT", moon.TerminationCode);
        Assert.Equal(new[] { "Moon", "Mars" }, moon.Chain);
    }

    private static ChartAnalysisInput Chart(params PlanetPosition[] planets) => new("D1", ZodiacName.Aries, planets.ToList());
    private static PlanetPosition P(string planet, string sign) => new() { Planet = planet, Sign = sign, PointKind = "Graha" };
}
