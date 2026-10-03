using Ikiastrro.Data.Statistics;

namespace Ikiastrro.Web.Tests;

public class AnalyticsSubjectRepositoryTests
{
    [Fact]
    public void EligibleRequiresExplicitPermission()
    {
        var update = new AnalyticsSubjectUpdate(1, "ELIGIBLE", "RESEARCH", "RECORDED", false);
        Assert.Throws<ArgumentException>(() => AnalyticsSubjectRepository.Validate(update));
    }

    [Fact]
    public void ExplicitEligiblePersonalRecordIsValid()
    {
        var update = new AnalyticsSubjectUpdate(1, "ELIGIBLE", "PERSONAL", "RECORDED", true);
        AnalyticsSubjectRepository.Validate(update);
    }

    [Fact]
    public void ExplicitEligibleResearchRecordIsValid()
    {
        var update = new AnalyticsSubjectUpdate(1, "ELIGIBLE", "RESEARCH", "RECORDED", true);
        AnalyticsSubjectRepository.Validate(update);
    }

    [Theory]
    [InlineData("TEST")]
    [InlineData("DEMO")]
    [InlineData("SYNTHETIC")]
    public void NonResearchFixturesCannotBeEligible(string classification)
    {
        var update = new AnalyticsSubjectUpdate(1, "ELIGIBLE", classification, "UNKNOWN", true);
        Assert.Throws<ArgumentException>(() => AnalyticsSubjectRepository.Validate(update));
    }
}
