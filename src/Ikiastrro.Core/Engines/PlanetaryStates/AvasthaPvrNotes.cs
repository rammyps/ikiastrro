namespace Ikiastrro.Core.Engines.PlanetaryStates;

/// <summary>
/// What each avastha state means, in P.V.R. Narasimha Rao's words, <i>Vedic Astrology: An Integrated Approach</i>
/// sec.15.4.1-15.4.3, printed pp.190-192 (<c>SRC_PVR_INTEGRATED</c>, checked against the raw extract), together
/// with the house-specific results he names there. Pure text; no I/O. The Śayanādi (15.4.4) results are per
/// planet and live in <c>tbl_Rule_PostureStateInterpretation</c>, not here.
/// <list type="bullet">
/// <item>Bālādi (age, 15.4.1): Saisava a quarter of its results, Kumaara half, Yuva all, Vriddha some, Mrita none.</item>
/// <item>Jāgradādi (alertness, 15.4.2): awake gives full results, dreaming medium, asleep negligible.</item>
/// <item>Dīptādi (attitude and mood, 15.4.3): nine states, then six more (Lajjita onward).</item>
/// </list>
/// State names are the project's spellings from <c>tbl_Dim_PlanetaryState</c> (PVR's Saisava is Baala here,
/// Kumaara is Kumara, Jaagrita is Jagrat).
/// </summary>
public static class AvasthaPvrNotes
{
    public const string Source = "PVR §15.4";

    /// <summary>PVR's caution after the age table (15.4.1): age is one factor among several.</summary>
    public const string AgeCaution =
        "PVR: do not be carried away with the age states; a planet can give good results irrespective of its " +
        "age-related state, as age is just one of the factors deciding its effectiveness.";

    private static readonly Dictionary<(string System, string State), string> Meanings = new()
    {
        [("Baaladi", "Baala")] = "child (Saisava): gives one quarter of its results",
        [("Baaladi", "Kumara")] = "adolescent (Kumaara): gives half of its results",
        [("Baaladi", "Yuva")] = "youth: gives all of its results",
        [("Baaladi", "Vriddha")] = "old: gives some of its results and is not very effective",
        [("Baaladi", "Mrita")] = "dead: gives none of its results",

        [("Jagradadi", "Jagrat")] = "awake, in an exaltation or own rasi: gives full results",
        [("Jagradadi", "Swapna")] = "dreaming, in a neutral or friendly planet's rasi: gives medium results",
        [("Jagradadi", "Sushupti")] = "asleep, in its debilitation rasi or an enemy's: its results are negligible",

        [("Deeptadi", "Deepta")] = "bright: in its exaltation rasi",
        [("Deeptadi", "Swastha")] = "doing well, contented, comfortable, natural: in its own rasi",
        [("Deeptadi", "Mudita")] = "delighted: in a good friend's rasi",
        [("Deeptadi", "Saanta")] = "peaceful: in a friend's rasi",
        [("Deeptadi", "Deena")] = "sad, depressed: in a neutral planet's rasi",
        [("Deeptadi", "Duhkhita")] = "distressed, miserable: in an enemy's rasi",
        [("Deeptadi", "Vikala")] = "crippled, confused: joined by malefic planets",
        [("Deeptadi", "Khala")] = "mischievous, scheming: in a malefic planet's rasi",
        [("Deeptadi", "Kopita")] = "angry: joined closely by the Sun",

        [("Lajjitadi", "Lajjita")] = "ashamed: in the 5th house joined by the Sun, Mars, Saturn, Rahu or Ketu",
        [("Lajjitadi", "Garvita")] = "proud: in its exaltation or moolatrikona rasi",
        [("Lajjitadi", "Kshudhita")] = "hungry: in an enemy's rasi, or conjoined or aspected by enemies, or conjoined by Saturn",
        [("Lajjitadi", "Trishita")] = "thirsty: in a watery rasi and aspected by enemies without the aspect of benefics",
        [("Lajjitadi", "Mudita")] = "delighted: in a friend's sign, conjoined or aspected by friends and conjoined by Jupiter",
        [("Lajjitadi", "Kshobhita")] = "shaken, agitated: conjoined by the Sun and aspected by malefics or enemies",
    };

    /// <summary>PVR's meaning of one state, or null when the book gives none for it (the Śayanādi states).</summary>
    public static string? Meaning(string avasthaSystem, string stateName) =>
        Meanings.GetValueOrDefault((avasthaSystem, stateName));

    /// <summary>
    /// The results PVR names for a planet in these states in a given house (15.4.3, last paragraph): a planet in
    /// Lajjita in the 5th may bring losses related to progeny; one in Kshobhita in the 7th may bring loss of
    /// spouse; and a house holding planets in Kshudhita or Kshobhita has its significations affected.
    /// </summary>
    public static IReadOnlyList<string> HouseResults(IEnumerable<string> lajjitadiStateNames, int house)
    {
        var states = lajjitadiStateNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var results = new List<string>();
        if (house == 5 && states.Contains("Lajjita")) results.Add("Lajjita in the 5th: there may be losses related to progeny.");
        if (house == 7 && states.Contains("Kshobhita")) results.Add("Kshobhita in the 7th: there may be loss of spouse.");
        if (states.Contains("Kshudhita") || states.Contains("Kshobhita"))
            results.Add($"Kshudhita or Kshobhita in house {house}: the significations of that house are affected.");
        return results;
    }
}
