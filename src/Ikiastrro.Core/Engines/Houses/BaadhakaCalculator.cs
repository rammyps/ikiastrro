using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Houses;

/// <summary>Baadhaka sthaana ("troubling spot") and baadhaka (its lord, the "troublemaker") for a
/// given rasi — P.V.R. Narasimha Rao, Vedic Astrology: An Integrated Approach, §13.3 (Table 31,
/// SRC_PVR_INTEGRATED). For a house falling in a movable/fixed/dual rasi, the 11th/9th/7th house
/// (respectively) from it is the baadhaka sthaana; its lord is the baadhaka for the original house.
/// PVR: "we can consider baadhaka from every house and arudha pada in every divisional chart" — so
/// the input here is deliberately just the sign the house/arudha occupies, not a Lagna: it is
/// reusable for any house, in any varga, the same way <see cref="HouseEngine.GetSignLord"/> is.
///
/// Divergence: PVR's own Table 31 additionally names Rahu as a co-baadhaka wherever the sthaana
/// lands in Aquarius (Aries' row) and Ketu wherever it lands in Scorpio (Capricorn's row), matching
/// his Table 6 attribution of Aquarius/Scorpio co-ownership to Rahu/Ketu. This project follows the
/// classical 7-planet sign rulership already used everywhere else (<see cref="HouseEngine.GetSignLord"/>,
/// <see cref="LagnaFunctionalNature"/>) — see the open Table 6 verification item in
/// docs/research/domain/pvr-coverage.md Ch. 3 — so only Saturn/Mars are returned for those two rows.
/// Rahu/Ketu are otherwise out of scope here, same as in <see cref="LagnaFunctionalNature"/>: they
/// own no sign and so are never themselves a baadhaka lord under this project's rulership table.</summary>
public static class BaadhakaCalculator
{
    private static readonly HashSet<ZodiacName> Movable = new()
        { ZodiacName.Aries, ZodiacName.Cancer, ZodiacName.Libra, ZodiacName.Capricornus };

    private static readonly HashSet<ZodiacName> Fixed = new()
        { ZodiacName.Taurus, ZodiacName.Leo, ZodiacName.Scorpio, ZodiacName.Aquarius };

    // Dual (Gemini, Virgo, Sagittarius, Pisces) is the implicit remainder — every ZodiacName value
    // is movable, fixed or dual, so this three-way split is exhaustive.

    /// <summary>Movable/Fixed/Dual classification of a rasi — same classical split as
    /// tbl_SignAttributes.type_house_keyattri (Chara/Sthira/Dwiswabhava). Shared with
    /// <see cref="RasiDrishtiCalculator"/> so the movable/fixed/dual sign lists
    /// exist exactly once in C#.</summary>
    public static SignModality GetModality(ZodiacName sign) =>
        Movable.Contains(sign) ? SignModality.Movable
            : Fixed.Contains(sign) ? SignModality.Fixed
            : SignModality.Dual;

    /// <summary>Baadhaka sthaana + baadhaka lord for a house/arudha occupying <paramref name="sign"/>,
    /// independent of any Lagna — reads directly off PVR Table 31.</summary>
    public static BaadhakaResult For(ZodiacName sign)
    {
        var modality = GetModality(sign);
        var sthaanaOffset = modality switch
        {
            SignModality.Movable => 11,
            SignModality.Fixed => 9,
            _ => 7
        };
        var sthaanaSign = HouseEngine.GetHouseSign(sign, sthaanaOffset);
        var baadhakaLord = HouseEngine.GetSignLord(sthaanaSign);

        return new BaadhakaResult(sign, modality, sthaanaOffset, sthaanaSign, baadhakaLord);
    }

    /// <summary>Convenience overload for the common case of reading baadhaka for house
    /// <paramref name="houseNumber"/> (1-12, Whole Sign) from a chart's own <paramref name="lagnaSign"/>
    /// — resolves the house's sign, then delegates to <see cref="For(ZodiacName)"/>, additionally
    /// reporting which house (from the same Lagna) the sthaana itself falls in. That house number is
    /// what PVR means by "the periods of [the baadhaka] and planets in [the sthaana sign] can create
    /// some obstructions" — it is the house/period to watch for trouble.</summary>
    public static BaadhakaResult For(ZodiacName lagnaSign, int houseNumber)
    {
        if (houseNumber is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(houseNumber), "House number must be 1-12 (Whole Sign).");

        var houseSign = HouseEngine.GetHouseSign(lagnaSign, houseNumber);
        var result = For(houseSign);
        var sthaanaHouseFromLagna = AstroMath.CountFromSignToSign(lagnaSign, result.SthaanaSign);
        return result with { SthaanaHouseNumberFromLagna = sthaanaHouseFromLagna };
    }
}

/// <summary>Which of the three classical modalities a rasi belongs to — drives the 11th/9th/7th
/// baadhaka-sthaana rule in §13.3.</summary>
public enum SignModality { Movable, Fixed, Dual }

/// <param name="Sign">The rasi/house sign being evaluated for baadhaka.</param>
/// <param name="Modality">Movable/Fixed/Dual — which count rule (11th/9th/7th) applied.</param>
/// <param name="SthaanaOffset">11, 9 or 7 — the house-count used to reach the sthaana from Sign.</param>
/// <param name="SthaanaSign">The baadhaka sthaana ("troubling spot") reached by that count.</param>
/// <param name="Baadhaka">The planet ruling <see cref="SthaanaSign"/> — the troublemaker for
/// <see cref="Sign"/>.</param>
/// <param name="SthaanaHouseNumberFromLagna">Only set by the <c>For(lagnaSign, houseNumber)</c>
/// overload: which house (1-12) from that Lagna the sthaana sign itself occupies.</param>
public record BaadhakaResult(
    ZodiacName Sign,
    SignModality Modality,
    int SthaanaOffset,
    ZodiacName SthaanaSign,
    string Baadhaka,
    int? SthaanaHouseNumberFromLagna = null);
