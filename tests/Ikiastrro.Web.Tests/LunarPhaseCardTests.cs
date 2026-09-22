using Bunit;
using Ikiastrro.Core.Models;
using Ikiastrro.Web.Components.Charts;
using Xunit;

namespace Ikiastrro.Web.Tests;

public class LunarPhaseCardTests : BunitContext
{
    [Fact]
    public void RendersWaxingGibbousPhaseAndPersistedPakshaBala()
    {
        var moon = new MoonContext
        {
            ElongationDegrees = 135,
            TithiNumber = 12,
            PakshaCode = "SHUKLA",
            IsWaxingMoon = true,
        };

        var cut = Render<LunarPhaseCard>(parameters => parameters
            .Add(p => p.Moon, moon)
            .Add(p => p.PakshaBalaVirupas, 45m));

        Assert.Contains("Waxing Gibbous", cut.Markup);
        Assert.Contains("Śukla Pakṣa", cut.Markup);
        Assert.Contains("45.00 virūpas", cut.Markup);
        Assert.Contains("aria-valuenow=\"45\"", cut.Markup);
    }

    [Fact]
    public void LabelsWaningCrescentFromElongation()
    {
        var moon = new MoonContext
        {
            ElongationDegrees = 315,
            TithiNumber = 27,
            PakshaCode = "KRISHNA",
            IsWaxingMoon = false,
        };

        var cut = Render<LunarPhaseCard>(parameters => parameters.Add(p => p.Moon, moon));

        Assert.Contains("Waning Crescent", cut.Markup);
        Assert.Contains("Kṛṣṇa Pakṣa", cut.Markup);
    }
}
