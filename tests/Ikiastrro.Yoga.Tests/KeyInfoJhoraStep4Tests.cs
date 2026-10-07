using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Karakas;
using Ikiastrro.Core.Engines.KeyInfo;

namespace Ikiastrro.Yoga.Tests;

/// <summary>JHora gap step 4 — special tārās, lattā, special tithis and the 64th navamsa / 22nd drekkana lords —
/// against JHora's own Basics ▸ Key Info views for 1_Ramakrishnan (explorer_output/jhora/basics-views,
/// 2026-09-23), fed JHora's own longitudes.</summary>
public sealed class KeyInfoJhoraStep4Tests
{
    private const double Lagna = 0.6574, Sun = 8.2019, Moon = 217.2088, Mars = 3.9433, Mercury = 1.8326,
        Jupiter = 158.7238, Venus = 11.9832, Saturn = 160.9595, Rahu = 102.9146;

    [Fact]
    public void SpecialTarasFromMoon()
    {
        var rows = SpecialTara.FromLongitude(Moon);
        var expected = new (string, ConstellationName, PlanetName)[]
        {
            ("Janma", ConstellationName.Anuradha, PlanetName.Saturn),
            ("Karma", ConstellationName.Poorvabhadra, PlanetName.Jupiter),
            ("Samudayika", ConstellationName.Aridra, PlanetName.Rahu),
            ("Sanghatika", ConstellationName.Rohini, PlanetName.Moon),
            ("Jaati", ConstellationName.Poorvashada, PlanetName.Venus),
            ("Naidhana", ConstellationName.Sravana, PlanetName.Moon),
            ("Desa", ConstellationName.Revathi, PlanetName.Mercury),
            ("Abhisheka", ConstellationName.Vishhaka, PlanetName.Jupiter),
            ("Aadhaana", ConstellationName.Punarvasu, PlanetName.Jupiter),
            ("Vainasika", ConstellationName.Makha, PlanetName.Ketu),
            ("Maanasa", ConstellationName.Hasta, PlanetName.Moon),
        };
        Assert.Equal(expected.Length, rows.Count);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].Item1, rows[i].Name);
            Assert.Equal(expected[i].Item2, rows[i].Nakshatra);
            Assert.Equal(expected[i].Item3, rows[i].Lord);
        }
    }

    [Fact]
    public void SpecialTarasFromLagnaReachAbhijit()
    {
        var rows = SpecialTara.FromLongitude(Lagna);
        Assert.Equal(ConstellationName.Aswini, rows[0].Nakshatra);
        Assert.Equal(PlanetName.Ketu, rows[0].Lord);
        Assert.Equal(ConstellationName.Makha, rows[1].Nakshatra);
        Assert.Equal(ConstellationName.Jyesta, rows[2].Nakshatra);
        Assert.Equal(PlanetName.Mercury, rows[2].Lord);
        Assert.Equal(ConstellationName.Rohini, rows[4].Nakshatra);
        Assert.Equal(ConstellationName.Revathi, rows[7].Nakshatra);   // Abhisheka
        Assert.Equal(ConstellationName.Moola, rows[8].Nakshatra);     // Aadhaana
        Assert.True(rows[9].IsAbhijit);                              // Vainasika — Abhijit, Sun
        Assert.Null(rows[9].Nakshatra);
        Assert.Equal(PlanetName.Sun, rows[9].Lord);
        Assert.Equal(ConstellationName.Satabhisha, rows[10].Nakshatra);
        Assert.Equal(PlanetName.Rahu, rows[10].Lord);
    }

    [Theory]
    [InlineData(PlanetName.Sun, Sun, ConstellationName.Aswini, ConstellationName.Uttara)]
    [InlineData(PlanetName.Moon, Moon, ConstellationName.Anuradha, ConstellationName.Dhanishta)]
    [InlineData(PlanetName.Mars, Mars, ConstellationName.Aswini, ConstellationName.Krithika)]
    [InlineData(PlanetName.Mercury, Mercury, ConstellationName.Aswini, ConstellationName.Sravana)]
    [InlineData(PlanetName.Jupiter, Jupiter, ConstellationName.Uttara, ConstellationName.Anuradha)]
    [InlineData(PlanetName.Venus, Venus, ConstellationName.Aswini, ConstellationName.Satabhisha)]
    [InlineData(PlanetName.Saturn, Saturn, ConstellationName.Hasta, ConstellationName.Poorvashada)]
    [InlineData(PlanetName.Rahu, Rahu, ConstellationName.Pushyami, ConstellationName.Revathi)]
    public void LattaStarMatchesJhora(PlanetName planet, double longitude, ConstellationName occupied, ConstellationName latta)
    {
        var row = LattaStar.Row(planet, longitude);
        Assert.Equal(occupied, row.Occupied);
        Assert.Equal(latta, row.Latta);
    }

    [Fact]
    public void SpecialTithisMatchJhoraAllThirtySix()
    {
        var rows = SpecialTithi.Compute(Sun, Moon);
        var tithis = new[]
        {
            18, 5, 23, 10, 28, 15, 2, 20, 7, 25, 12, 30, 17, 4, 22, 9, 27, 14, 1, 19, 6, 24, 11, 29,
            16, 3, 21, 8, 26, 13, 30, 18, 5, 23, 10, 28,
        };
        Assert.Equal(36, rows.Count);
        for (var i = 0; i < 36; i++) Assert.Equal(tithis[i], rows[i].TithiNumber);
    }

    [Fact]
    public void SpecialTithiDetailMatchesJhora()
    {
        var rows = SpecialTithi.Compute(Sun, Moon);

        Assert.Equal("Janma", rows[0].Name);
        Assert.Equal("Krishna Tritiya", rows[0].TithiName);
        Assert.InRange(rows[0].PercentLeft, 57.5, 58.5);   // JHora prints whole percent
        Assert.Equal(PlanetName.Mars, rows[0].Lord);
        Assert.Equal("Nitya Klinna", rows[0].Deity);
        Assert.Equal(3, rows[0].IndexInPaksha);
        Assert.False(rows[0].Bright);

        Assert.Equal("Sukla Panchami", rows[1].TithiName);
        Assert.Equal(PlanetName.Jupiter, rows[1].Lord);
        Assert.Equal("Vahni Vaasini", rows[1].Deity);

        Assert.Equal("Pournimasya", rows[5].TithiName);        // Satru
        Assert.Equal(PlanetName.Saturn, rows[5].Lord);
        Assert.Equal("Chitra", rows[5].Deity);

        Assert.Equal("Amavasya", rows[11].TithiName);          // Vyaya
        Assert.Equal(PlanetName.Rahu, rows[11].Lord);

        Assert.Equal("Krishna Navami", rows[21].TithiName);    // Karma, cycle 2
        Assert.Equal(PlanetName.Sun, rows[21].Lord);
        Assert.Equal("Karma (cycle 2)", rows[21].Name);

        Assert.Equal("Vyaya (cycle 3)", rows[35].Name);
        Assert.Equal("Krishna Trayodasi", rows[35].TithiName);
        Assert.Equal(PlanetName.Jupiter, rows[35].Lord);
    }

    [Fact]
    public void LordsOf64thNavamsaAndSixtyFourthDrekkanaMatchJhora()
    {
        Assert.Equal(PlanetName.Moon, MaliciousDivisions.Lord64thNavamsa(Lagna));
        Assert.Equal(PlanetName.Jupiter, MaliciousDivisions.Lord64thNavamsa(Moon));
    }

    [Theory]
    [InlineData(DrekkanaScheme.Parasara, PlanetName.Mars, PlanetName.Mercury)]
    [InlineData(DrekkanaScheme.ParivrittiTraya, PlanetName.Saturn, PlanetName.Venus)]
    [InlineData(DrekkanaScheme.Somanatha, PlanetName.Mercury, PlanetName.Moon)]
    [InlineData(DrekkanaScheme.UmaShambhu, PlanetName.Jupiter, PlanetName.Venus)]
    [InlineData(DrekkanaScheme.Jagannatha, PlanetName.Moon, PlanetName.Venus)]
    public void LordOf22ndDrekkanaMatchesJhora(DrekkanaScheme scheme, PlanetName fromLagna, PlanetName fromMoon)
    {
        Assert.Equal(fromLagna, MaliciousDivisions.Lord22ndDrekkana(scheme, Lagna));
        Assert.Equal(fromMoon, MaliciousDivisions.Lord22ndDrekkana(scheme, Moon));
    }
}

public sealed class SahamTableJhoraTests
{
    private static readonly Dictionary<PlanetName, double> Grahas = new()
    {
        [PlanetName.Sun] = 8.2019, [PlanetName.Moon] = 217.2088, [PlanetName.Mars] = 3.9433,
        [PlanetName.Mercury] = 1.8326, [PlanetName.Jupiter] = 158.7238, [PlanetName.Venus] = 11.9832,
        [PlanetName.Saturn] = 160.9595,
    };

    // JHora "Sahamas (sahams)" for 1_Ramakrishnan — a night birth (05:30, sunrise 05:56) — to 0.01 arcsec.
    private static readonly (string Name, double Longitude)[] Jhora =
    {
        ("Punya", 151.65046),
        ("Vidya", 239.66426),
        ("Yasas", 353.58405),
        ("Mitra", 4.90986),
        ("Mahatmya", 212.95018),
        ("Asha", 203.64113),
        ("Samartha", 205.87687),
        ("Bhratru", 358.42162),
        ("Gaurava", 96.68684),
        ("Pitru", 207.89970),
        ("Rajya", 207.89970),
        ("Matru", 155.43178),
        ("Putra", 89.14234),
        ("Jeeva", 358.42162),
        ("Karma", 358.54665),
        ("Roga", 144.10597),
        ("Kali", 205.87687),
        ("Sastra", 34.06830),
        ("Bandhu", 246.03356),
        ("Mrityu", 354.10597),
        ("Paradesa", 112.59095),
        ("Artha", 49.33155),
        ("Paradara", 356.87604),
        ("Vanik", 145.28117),
        ("Karyasiddhi", 307.69403),
        ("Vivaha", 179.63370),
        ("Santapa", 236.90661),
        ("Sraddha", 352.61746),
        ("Preeti", 148.23952),
        ("Jadya", 188.84880),
        ("Vyapara", 203.64113),
        ("Satru", 187.67360),
        ("Jalapatana", 86.61687),
        ("Bandhana", 39.96641),
        ("Apamrityu", 153.94328),
        ("Labha", 170.35521),
    };

    [Fact]
    public void AllThirtySixSahamsMatchJhoraForANightBirth()
    {
        var sahams = SahamTable.Compute(Grahas, 0.6574, isNightBirth: true);
        Assert.Equal(36, sahams.Count);
        for (var i = 0; i < 36; i++)
        {
            Assert.StartsWith(Jhora[i].Name, sahams[i].Name, StringComparison.OrdinalIgnoreCase);
            var diff = Math.Abs(AstroMath.Normalize(sahams[i].LongitudeDegrees - Jhora[i].Longitude + 180) - 180);
            Assert.True(diff < 0.003, $"{Jhora[i].Name}: ours {sahams[i].LongitudeDegrees:F4}, JHora {Jhora[i].Longitude:F4}");
        }
    }

    [Fact]
    public void ADayBirthSwapsTheNightFormulas()
    {
        // Vidya is Sun − Moon + Lagna by day, Moon − Sun + Lagna by night.
        var day = SahamTable.Compute(Grahas, 0.6574, isNightBirth: false).First(s => s.Code == "VIDYA");
        var night = SahamTable.Compute(Grahas, 0.6574, isNightBirth: true).First(s => s.Code == "VIDYA");
        Assert.NotEqual(day.LongitudeDegrees, night.LongitudeDegrees, 3);
        // Bhratru is the same by day and night.
        Assert.Equal(
            SahamTable.Compute(Grahas, 0.6574, true).First(s => s.Code == "BHRATRU").LongitudeDegrees,
            SahamTable.Compute(Grahas, 0.6574, false).First(s => s.Code == "BHRATRU").LongitudeDegrees, 6);
    }
}
