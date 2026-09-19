using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using static Ikiastrro.Core.Engines.Houses.ArgalaCalculator;

namespace Ikiastrro.Web.Tests;

/// <summary>
/// Verification fixture: P.V.R. Narasimha Rao, Vedic Astrology: An Integrated Approach,
/// Exercise 16 (sec.10.6, pp.110-111) — "Find the planets causing argalas and virodhargalas on
/// all the 12 houses in Chart 5." Chart 5's own positions (Lagna Scorpio) were reconstructed
/// from the book's Exercise 14 (graha-drishti) answer table plus its explicit "Ketu is in Aq"
/// note, then cross-checked against the still-legible rows of the book's own (OCR-scrambled)
/// answer table for Exercise 16 — houses 1, 4, 10, 11 and 12 matched exactly, including the
/// malefic-in-3rd exception on house 11. See docs/research/domain/
/// argala-virodhargala-drishti-lifematters.md sec.4 for the full derivation.
/// </summary>
public sealed class ArgalaCalculatorTests
{
    // Chart 5, Lagna = Scorpio. Houses 5, 8, 9, 12 are empty.
    private static readonly IReadOnlyDictionary<ZodiacName, IReadOnlyList<PlanetName>> Chart5 =
        new Dictionary<ZodiacName, IReadOnlyList<PlanetName>>
        {
            [ZodiacName.Scorpio] = new[] { PlanetName.Mars, PlanetName.Saturn },      // house 1
            [ZodiacName.Sagittarius] = new[] { PlanetName.Mercury },                  // house 2
            [ZodiacName.Capricornus] = new[] { PlanetName.Venus },                    // house 3
            [ZodiacName.Aquarius] = new[] { PlanetName.Ketu },                        // house 4
            [ZodiacName.Aries] = new[] { PlanetName.Moon },                           // house 6
            [ZodiacName.Taurus] = new[] { PlanetName.Sun },                           // house 7
            [ZodiacName.Leo] = new[] { PlanetName.Rahu },                             // house 10
            [ZodiacName.Virgo] = new[] { PlanetName.Jupiter },                        // house 11
        };

    private static IReadOnlyList<PlanetName> Occupants(ArgalaEvaluation eval, RelationType side, int offset) =>
        (side == RelationType.Argala ? eval.Argala : eval.Virodhargala)
            .Single(p => p.Position.HouseOffset == offset).Occupants;

    private static void AssertOccupants(ArgalaEvaluation eval, RelationType side, int offset, params PlanetName[] expected) =>
        Assert.Equal(expected, Occupants(eval, side, offset));

    // ---- House 1 (Sc): fully cross-checked against the book's own answer-key fragment ----
    [Fact]
    public void House1_Matches_Book_Answer()
    {
        var eval = Evaluate(ZodiacName.Scorpio, Chart5);

        AssertOccupants(eval, RelationType.Argala, 2, PlanetName.Mercury);
        AssertOccupants(eval, RelationType.Argala, 4, PlanetName.Ketu);
        AssertOccupants(eval, RelationType.Argala, 5);
        AssertOccupants(eval, RelationType.Argala, 11, PlanetName.Jupiter);
        AssertOccupants(eval, RelationType.Virodhargala, 12);
        AssertOccupants(eval, RelationType.Virodhargala, 10, PlanetName.Rahu);
        AssertOccupants(eval, RelationType.Virodhargala, 9);
        AssertOccupants(eval, RelationType.Virodhargala, 3, PlanetName.Venus);
        Assert.False(eval.ThirdHouseExceptionApplied);
        Assert.False(eval.CountedAntiZodiacally);
    }

    // ---- House 4 (Aq): holds Ketu -> anti-zodiacal counting, cross-checked against the book ----
    [Fact]
    public void House4_Counts_AntiZodiacally_Because_Ketu_Occupies_It()
    {
        var eval = Evaluate(ZodiacName.Aquarius, Chart5);

        Assert.True(eval.CountedAntiZodiacally);
        AssertOccupants(eval, RelationType.Argala, 2, PlanetName.Venus);
        AssertOccupants(eval, RelationType.Argala, 4, PlanetName.Mars, PlanetName.Saturn);
        AssertOccupants(eval, RelationType.Argala, 5);
        AssertOccupants(eval, RelationType.Argala, 11, PlanetName.Moon);
        AssertOccupants(eval, RelationType.Virodhargala, 12);
        AssertOccupants(eval, RelationType.Virodhargala, 10, PlanetName.Sun);
        AssertOccupants(eval, RelationType.Virodhargala, 9);
        AssertOccupants(eval, RelationType.Virodhargala, 3, PlanetName.Mercury);
    }

    // ---- House 10 (Le): fully cross-checked (7/8 columns exact, 8th an OCR-plausible match) ----
    [Fact]
    public void House10_Matches_Book_Answer()
    {
        var eval = Evaluate(ZodiacName.Leo, Chart5);

        AssertOccupants(eval, RelationType.Argala, 2, PlanetName.Jupiter);
        AssertOccupants(eval, RelationType.Argala, 4, PlanetName.Mars, PlanetName.Saturn);
        AssertOccupants(eval, RelationType.Argala, 5, PlanetName.Mercury);
        AssertOccupants(eval, RelationType.Argala, 11);
        AssertOccupants(eval, RelationType.Virodhargala, 12);
        AssertOccupants(eval, RelationType.Virodhargala, 10, PlanetName.Sun);
        AssertOccupants(eval, RelationType.Virodhargala, 9, PlanetName.Moon);
        AssertOccupants(eval, RelationType.Virodhargala, 3);
    }

    // ---- House 11 (Vi): the sec.10.6 "2+ malefics in 3rd" exception fires here (Mars+Saturn) ----
    [Fact]
    public void House11_ThirdHouse_Exception_Moves_MarsSaturn_From_Virodhargala_To_Argala()
    {
        var eval = Evaluate(ZodiacName.Virgo, Chart5);

        Assert.True(eval.ThirdHouseExceptionApplied);
        Assert.DoesNotContain(eval.Virodhargala, p => p.Position.HouseOffset == 3);
        AssertOccupants(eval, RelationType.Argala, 2);
        AssertOccupants(eval, RelationType.Argala, 4, PlanetName.Mercury);
        AssertOccupants(eval, RelationType.Argala, 5, PlanetName.Venus);
        AssertOccupants(eval, RelationType.Argala, 11);
        Assert.Equal(new[] { PlanetName.Mars, PlanetName.Saturn },
            eval.Argala.Single(p => p.Position.HouseOffset == 3).Occupants);
        AssertOccupants(eval, RelationType.Virodhargala, 12, PlanetName.Rahu);
        AssertOccupants(eval, RelationType.Virodhargala, 10);
        AssertOccupants(eval, RelationType.Virodhargala, 9, PlanetName.Sun);
    }

    // ---- House 12 (Li): fully cross-checked against the book's own answer-key fragment ----
    [Fact]
    public void House12_Matches_Book_Answer()
    {
        var eval = Evaluate(ZodiacName.Libra, Chart5);

        AssertOccupants(eval, RelationType.Argala, 2, PlanetName.Mars, PlanetName.Saturn);
        AssertOccupants(eval, RelationType.Argala, 4, PlanetName.Venus);
        AssertOccupants(eval, RelationType.Argala, 5, PlanetName.Ketu);
        AssertOccupants(eval, RelationType.Argala, 11, PlanetName.Rahu);
        AssertOccupants(eval, RelationType.Virodhargala, 12, PlanetName.Jupiter);
        AssertOccupants(eval, RelationType.Virodhargala, 10);
        AssertOccupants(eval, RelationType.Virodhargala, 9);
        AssertOccupants(eval, RelationType.Virodhargala, 3, PlanetName.Mercury);
    }

    // ---- House-number overload resolves the same as passing the sign directly ----
    [Fact]
    public void HouseNumber_Overload_Matches_Direct_Sign_Overload()
    {
        var byHouseNumber = Evaluate(ZodiacName.Scorpio, 1, Chart5);
        var bySign = Evaluate(ZodiacName.Scorpio, Chart5);

        Assert.Equal(bySign.Argala.SelectMany(p => p.Occupants), byHouseNumber.Argala.SelectMany(p => p.Occupants));
        Assert.Equal(bySign.Virodhargala.SelectMany(p => p.Occupants), byHouseNumber.Virodhargala.SelectMany(p => p.Occupants));
    }

    // ---- sec.10.7 comparison method: count first, dignity-sum tie-break (PROJECT_SYNTHESIS) ----
    [Fact]
    public void Compare_Prefers_The_Side_With_More_Occupants()
    {
        // house 1: argala = Mercury, Ketu, Jupiter (3); virodhargala = Rahu, Venus (2)
        var eval = Evaluate(ZodiacName.Scorpio, Chart5);
        var result = Compare(eval, _ => 0);

        Assert.Equal(3, result.ArgalaCount);
        Assert.Equal(2, result.VirodhargalaCount);
        Assert.Equal(RelationType.Argala, result.Dominant);
    }

    [Fact]
    public void Compare_Falls_Back_To_Dignity_Sum_When_Counts_Tie()
    {
        var eval = Evaluate(ZodiacName.Aquarius, Chart5); // house 4: argala side [0] = Venus(2nd), virodh non-empty = Sun(10th)
        var tied = eval with
        {
            Argala = new[] { eval.Argala[0] },
            Virodhargala = new[] { eval.Virodhargala.First(p => p.Occupants.Count > 0) }
        };

        var result = Compare(tied, planet => planet == PlanetName.Venus ? 2 : -2);

        Assert.Equal(result.ArgalaCount, result.VirodhargalaCount);
        Assert.NotEqual(0, result.ArgalaDignitySum - result.VirodhargalaDignitySum);
        Assert.Equal(RelationType.Argala, result.Dominant);
    }
}
