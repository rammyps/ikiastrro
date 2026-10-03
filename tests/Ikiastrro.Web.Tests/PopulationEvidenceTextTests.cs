using Ikiastrro.Data.Statistics;
using Ikiastrro.Web.Components.LifeMatters;

namespace Ikiastrro.Web.Tests;

public class PopulationEvidenceTextTests
{
    [Fact]
    public void NotEnrolled_IsExplicitAndDoesNotImplyConsent()
    {
        var snapshot = Snapshot("NOT_ENROLLED", 0);
        Assert.Contains("not enrolled", PopulationEvidenceText.Message(snapshot), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EmptySmallCohort_ShowsInsufficientReferenceData()
    {
        var snapshot = Snapshot("NO_COMPARISONS", 6);
        var message = PopulationEvidenceText.Message(snapshot);
        Assert.Contains("Insufficient reference data", message);
        Assert.Contains("6 eligible people", message);
        Assert.Contains("30", message);
    }

    [Fact]
    public void NullPercentile_IsNeverRenderedAsZero()
    {
        Assert.Equal("Not published", PopulationEvidenceText.Percentile(null));
    }

    private static PopulationEvidenceSnapshot Snapshot(string code, int eligiblePeople) =>
        new(code, null, null, eligiblePeople, 1, "KI_D1_HOUSE_STRENGTH", 1,
            "KI_D1_HOUSE_STRENGTH_V1", 1, DateTime.UtcNow, []);
}
