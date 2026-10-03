using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Matching;

/// <summary>What the pair similarity reads for one person: the D1 Lagna and signs, the Moon's nakshatra
/// facts, the Moon's pada, and the D9 when it is stored.</summary>
public sealed record SimilarityInput(DoshaChart D1, MatchPerson Moon, int Pada, NavamsaChart? D9);

/// <summary>One thing the two charts have in common, grouped by what kind of fact it is.</summary>
public sealed record SharedFact(string Category, string Text);

/// <summary>
/// What two charts share, as plain coincidences: the same Lagna, Moon or Sun sign, the same nakshatra or
/// pada, the same Gana, Yoni animal or Nadi, each graha in the same D1 sign, the same D9 Lagna, each graha in
/// the same D9 sign, and one person's Lagna sign being the other's Moon sign. It is a count of shared facts
/// and not a score: how common each coincidence is in the general population is not measured yet (base
/// rates are a later step in docs/architecture/compatibility_similarity.md section 5.3), so nothing here is
/// presented as evidence of a connection. Pure; no I/O.
/// </summary>
public static class ChartSimilarity
{
    public static IReadOnlyList<SharedFact> Compare(SimilarityInput a, SimilarityInput b, string nameA, string nameB)
    {
        var shared = new List<SharedFact>();
        void Add(string category, string text) => shared.Add(new(category, text));
        string Sign(ZodiacName s) => SignLabels.For(s);

        if (a.D1.Lagna == b.D1.Lagna) Add("Lagna", $"Both have {Sign(a.D1.Lagna)} Lagna.");
        if (a.D1.Signs[PlanetName.Moon] == b.D1.Signs[PlanetName.Moon]) Add("Moon", $"Both have the Moon in {Sign(a.D1.Signs[PlanetName.Moon])}.");
        if (a.D1.Signs[PlanetName.Sun] == b.D1.Signs[PlanetName.Sun]) Add("Sun", $"Both have the Sun in {Sign(a.D1.Signs[PlanetName.Sun])}.");

        if (a.Moon.NakshatraNumber == b.Moon.NakshatraNumber)
        {
            Add("Nakshatra", "Both have the same Moon nakshatra.");
            if (a.Pada == b.Pada) Add("Nakshatra", $"Both are in pada {a.Pada} of it.");
        }
        if (string.Equals(a.Moon.Gana, b.Moon.Gana, StringComparison.OrdinalIgnoreCase)) Add("Moon nature", $"Both are {a.Moon.Gana} gana.");
        if (string.Equals(a.Moon.YoniAnimal, b.Moon.YoniAnimal, StringComparison.OrdinalIgnoreCase)) Add("Moon nature", $"Both have the {a.Moon.YoniAnimal} Yoni.");
        if (string.Equals(a.Moon.Nadi, b.Moon.Nadi, StringComparison.OrdinalIgnoreCase)) Add("Moon nature", $"Both are {a.Moon.Nadi} nadi.");

        foreach (var p in Enum.GetValues<PlanetName>())
            if (p is not (PlanetName.Moon or PlanetName.Sun) && a.D1.Signs[p] == b.D1.Signs[p])
                Add("Grahas in D1", $"{p} is in {Sign(a.D1.Signs[p])} in both.");

        if (a.D1.Lagna == b.D1.Signs[PlanetName.Moon] && a.D1.Lagna != b.D1.Lagna)
            Add("Cross", $"{nameA}'s Lagna sign ({Sign(a.D1.Lagna)}) is {nameB}'s Moon sign.");
        if (b.D1.Lagna == a.D1.Signs[PlanetName.Moon] && a.D1.Lagna != b.D1.Lagna)
            Add("Cross", $"{nameB}'s Lagna sign ({Sign(b.D1.Lagna)}) is {nameA}'s Moon sign.");

        if (a.D9 is { } da && b.D9 is { } db)
        {
            if (da.D9Lagna == db.D9Lagna) Add("Navamsa", $"Both have {Sign(da.D9Lagna)} D9 Lagna.");
            foreach (var p in Enum.GetValues<PlanetName>())
                if (da.D9[p].Sign == db.D9[p].Sign)
                    Add("Grahas in D9", $"{p} is in {Sign(da.D9[p].Sign)} in the Navamsa of both.");
        }
        return shared;
    }
}
