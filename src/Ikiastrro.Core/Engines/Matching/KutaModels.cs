using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Matching;

/// <summary>Everything Kuta matching reads about one person: the Moon's sign and nakshatra, plus the
/// nakshatra attributes already stored on tbl_Nakshatras (migration 098). Gana, Yoni and Nadi are
/// inputs, not re-derived here, so the stored table stays the single source for them.</summary>
/// <param name="NakshatraNumber">1-27 (Ashwini = 1), equal to tbl_Nakshatras.Id.</param>
/// <param name="Gana">"Deva", "Manushya" or "Rakshasa".</param>
/// <param name="YoniAnimal">Animal of the nakshatra's Yoni, e.g. "Deer".</param>
/// <param name="Nadi">"Vata", "Pitta" or "Kapha".</param>
public sealed record MatchPerson(
    ZodiacName MoonSign, int NakshatraNumber, string Gana, string YoniAnimal, string YoniGender, string Nadi);

/// <summary>Present = the factor is satisfied; Absent = it is not; Unscored = the source gives no
/// rule that decides this case (the result carries no score and says why).</summary>
public enum KutaStatus { Present, Absent, Unscored }

/// <summary>One Kuta factor's reading. <see cref="Score"/> is null when <see cref="Status"/> is
/// Unscored, or for factors the source gives no numeric value (Rajju, Stree Deergha, Mahendra).</summary>
public sealed record KutaResult(
    string Code, string Name, int? Score, int MaxScore, KutaStatus Status, string Reason, string SourceLocator);

/// <summary>The eight scored Kutas (36 points) plus the unscored factors the source also tests.
/// <see cref="TotalMin"/>..<see cref="TotalMax"/> is the range the total can take: it is one value
/// unless a factor is Unscored, in which case TotalMax adds that factor's maximum.</summary>
public sealed record AshtakootaResult(
    IReadOnlyList<KutaResult> Scored,
    IReadOnlyList<KutaResult> Additional,
    int TotalMin,
    int TotalMax,
    bool SameSignLord)
{
    public const int MaxTotal = 36;
    /// <summary>The source's pass mark: 18 or more of 36.</summary>
    public const int PassMark = 18;

    public bool IsComplete => TotalMin == TotalMax;

    /// <summary>True/false once the range settles it against <see cref="PassMark"/>; null when the
    /// range straddles it.</summary>
    public bool? Passes => TotalMin >= PassMark ? true : TotalMax < PassMark ? false : null;
}
