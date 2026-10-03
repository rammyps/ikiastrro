namespace Ikiastrro.Core.Engines.Matching;

/// <summary>
/// Names the rules a compatibility reading was made under, so a copied or printed comparison can say which.
/// Bump <see cref="Version"/> (to the date) whenever a matching rule, table or weight changes, and say what
/// changed in the history below. The version is not stored with a pair: a pair result is a pure function of
/// two people and the rules (docs/architecture/compatibility_similarity.md, section 4.2).
///
/// History:
///  2026-10-04  Rajju, Stree Deergha (&gt;7) and Mahendra (13, not 12) scored 1 per pass; Yoni for different
///              animals from PyJHora's matrix; Navamsa, synastry and similarity added as facts only.
/// </summary>
public static class CompatibilityRuleSet
{
    public const string Version = "2026-10-04";

    public const string Sources =
        "Vasudev, The Art of Matching Charts, Ch. V and VI (Kuta, Dosha Samya); PyJHora South Indian scheme "
        + "(Yoni pair values; Rajju, Stree Deergha and Mahendra rules and 1-per-pass weight). Navamsa, synastry, "
        + "similarity and dasha timing are facts only.";
}
