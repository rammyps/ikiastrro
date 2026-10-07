using Bunit;
using Ikiastrro.Core.Models;
using Ikiastrro.Web.Components.Charts;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>The JHora gap step 4 tables (special tārās, lattā, special tithis, sphuṭas, sahams, malicious divisions)
/// render the figures the Core engines produce for 1_Ramakrishnan's D1 (engines are checked against JHora in
/// Ikiastrro.Yoga.Tests.KeyInfoJhoraStep4Tests).</summary>
public sealed class KeyInfoStep4TablesTests : BunitContext
{
    private static ChartKeyDetail Point(string kind, string name, double lon) => new()
    {
        PointKind = kind, Planet = name, NirayanaLongitudeDegrees = lon,
    };

    private static readonly ChartKeyDetail[] D1 =
    [
        Point("Graha", "Ascendant", 0.6574), Point("Graha", "Sun", 8.2019), Point("Graha", "Moon", 217.2088),
        Point("Graha", "Mars", 3.9433), Point("Graha", "Mercury", 1.8326), Point("Graha", "Jupiter", 158.7238),
        Point("Graha", "Venus", 11.9832), Point("Graha", "Saturn", 160.9595), Point("Graha", "Rahu", 102.9146),
        Point("Graha", "Ketu", 282.9146), Point("Upagraha", "Maandi", 187.5047),
    ];

    [Fact]
    public void SpecialTarasListElevenRowsAndReachAbhijitFromTheLagna()
    {
        var cut = Render<SpecialTaraTable>(p => p.Add(x => x.KeyDetails, D1));

        var rows = cut.FindAll("tbody tr");
        Assert.Equal(11, rows.Count);
        Assert.Contains("Janma", rows[0].Normalized());
        Assert.Contains("Anuradha", rows[0].Normalized());
        Assert.Contains("Abhijit", rows[9].Normalized());                       // Vainasika from the Lagna
        Assert.Contains("Naidhana", cut.Find(".kit-summary").Normalized());
    }

    [Fact]
    public void LattaListsNineGrahasAndNamesAStruckPlanet()
    {
        var cut = Render<LattaTable>(p => p.Add(x => x.KeyDetails, D1));

        var rows = cut.FindAll("tbody tr");
        Assert.Equal(9, rows.Count);
        Assert.Contains("Anuradha", rows[1].Normalized());   // Moon occupies Anuradha
        Assert.Contains("Dhanishta", rows[1].Normalized());  // and kicks Dhanishta
    }

    [Fact]
    public void SpecialTithisListAllThirtySixWithTheBirthTithiFirst()
    {
        var cut = Render<SpecialTithiTable>(p => p.Add(x => x.KeyDetails, D1));

        var rows = cut.FindAll("tbody tr");
        Assert.Equal(36, rows.Count);
        Assert.Contains("Janma", rows[0].Normalized());
        Assert.Contains("Krishna Tritiya", rows[0].Normalized());
        Assert.Contains("Amavasya", rows[11].Normalized());
    }

    [Fact]
    public void SphutasNeedMaandiAndListFourteenRowsWithIt()
    {
        var withGulika = Render<SphutaTable>(p => p.Add(x => x.KeyDetails, D1));
        Assert.Equal(14, withGulika.FindAll("tbody tr").Count);
        Assert.Contains("Prāṇa", withGulika.Find(".kit-summary").Normalized());

        var without = Render<SphutaTable>(p => p.Add(x => x.KeyDetails, D1.Where(k => k.Planet != "Maandi").ToList()));
        Assert.Contains("Needs", without.Markup);
        Assert.Empty(without.FindAll("tbody tr"));
    }

    [Fact]
    public void SahamsListThirtySixAndSayDayOrNight()
    {
        var night = Render<SahamsTable>(p => p.Add(x => x.KeyDetails, D1).Add(x => x.IsNightBirth, true));
        Assert.Equal(36, night.FindAll("tbody tr").Count);
        Assert.Contains("night", night.Find(".kit-summary").Normalized());
        Assert.Contains("Punya", night.FindAll("tbody tr")[0].Normalized());

        var day = Render<SahamsTable>(p => p.Add(x => x.KeyDetails, D1).Add(x => x.IsNightBirth, false));
        Assert.Contains("day", day.Find(".kit-summary").Normalized());
    }

    [Fact]
    public void MaliciousDivisionsShowTheNavamsaAndFiveDrekkanaSchemes()
    {
        var cut = Render<MaliciousDivisionsTable>(p => p.Add(x => x.KeyDetails, D1));

        var rows = cut.FindAll("tbody tr");
        Assert.Equal(6, rows.Count);
        Assert.Contains("64th navamsa", rows[0].Normalized());
        Assert.Contains("Moon", rows[0].Normalized());       // Lagna's 64th navamsa lord
        Assert.Contains("Jagannātha", rows[5].Normalized());
    }
}
