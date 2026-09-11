using Ikiastrro.Core.Engines.PlanetaryStates;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

/// <summary>PostureStateCalculator (Sayanaadi Avastha, PVR sec 15.4.4) against the JHora export
/// for 1_Ramakrishnan's printed Activity table. No DB dependency — pure arithmetic.</summary>
public class PostureStateCalculatorTests
{
    // Shared inputs for 1_Ramakrishnan: M = Anuraadha (17), G = ghati 59 (Janma Ghatis 58.8892),
    // L = Aries (1).
    private const int M = 17, G = 59, L = 1;

    [Fact]
    public void Sun_matches_jhora_aagama()
    {
        // Sun: C = Aswini (1), P = 1, A = 3rd navamsa (8.205deg in sign / (30/9) + 1 = 3).
        var navamsa = PostureStateCalculator.NavamsaIndex(8.205m);
        Assert.Equal(3, navamsa);

        var index = PostureStateCalculator.ComputeIndex(nakshatraNumber: 1, planetIndex: 1, navamsaIndex: navamsa, M, G, L);
        Assert.Equal(8, index); // Aagama
    }

    [Fact]
    public void Moon_matches_jhora_kautuka()
    {
        // Moon: C = Anuraadha (17), P = 2, A = 3rd navamsa (7.293deg in sign).
        var navamsa = PostureStateCalculator.NavamsaIndex(7.293m);
        Assert.Equal(3, navamsa);

        var index = PostureStateCalculator.ComputeIndex(nakshatraNumber: 17, planetIndex: 2, navamsaIndex: navamsa, M, G, L);
        Assert.Equal(11, index); // Kautuka
    }

    [Fact]
    public void GhatiRunning_takes_the_floor_plus_one()
    {
        // JHora: Janma Ghatis 58.8892 -> the 59th ghati is running (matches the book's own
        // footnote-52 worked example: 42.5 elapsed -> "the 43rd ghati was running").
        Assert.Equal(59, PostureStateCalculator.GhatiRunning(58.8892));
        Assert.Equal(43, PostureStateCalculator.GhatiRunning(42.5));
    }

    [Fact]
    public void ComputeIndex_maps_a_zero_remainder_to_12_not_0()
    {
        // C*P*A + M + G + L = 12 exactly -> remainder 0 -> Nidraa (index 12), not an invalid 0.
        var index = PostureStateCalculator.ComputeIndex(
            nakshatraNumber: 1, planetIndex: 1, navamsaIndex: 1, moonNakshatraNumber: 0, ghati: 0, lagnaRasiNumber: 11);
        Assert.Equal(12, index);
    }
}
