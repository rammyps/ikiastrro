using Bunit;
using Ikiastrro.Core.Presentation;
using Ikiastrro.Web.Components.Charts;
using Xunit;

namespace Ikiastrro.Web.Tests;

public sealed class PlanetDignityTableTests : BunitContext
{
    [Fact]
    public void AriesLagnaRendersCanonicalFunctionalRoleAndOwnershipEvidence()
    {
        var rows = new[]
        {
            new ExaltationRow("Mars", "Moolatrikona", "GK", "28° Capricorn", 66.0, 63),
            new ExaltationRow("Venus", "Enemy", "AmK", "27° Pisces", 15.0, 92),
            new ExaltationRow("Saturn", "Neutral", "BK", "20° Libra", 39.1, 78),
            new ExaltationRow("Rahu", "Neutral", "AK", null, null, null),
        };

        var cut = Render<PlanetDignityTable>(p => p
            .Add(x => x.Rows, rows)
            .Add(x => x.LagnaSign, "Aries"));

        Assert.Contains("Functional", cut.Markup);
        Assert.Contains("Dusthāna · H8", cut.Markup);
        Assert.Contains("Māraka · H2/H7", cut.Markup);
        Assert.Contains("Bādhaka · H11", cut.Markup);
        Assert.Contains("Triṣaḍāya · H11", cut.Markup);
        Assert.Contains("Rationale", cut.Markup);
        Assert.Contains("Rahu", cut.Markup);
    }
}
