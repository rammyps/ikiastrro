using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.KeyInfo;

/// <summary>One of the nine tārās: its name, meaning, Vimśottarī lord and its three nakṣatras.</summary>
public sealed record TaraRow(int Number, string Name, string Meaning, PlanetName Lord, IReadOnlyList<ConstellationName> Nakshatras);

/// <summary>
/// Nava tārā (SRC_PVR_INTEGRATED): counting from the reference nakṣatra (the Moon's — Janma — or the
/// Lagna's), the n-th, (n+9)-th and (n+18)-th nakṣatras share the n-th tārā. Each tārā's three
/// nakṣatras share one Vimśottarī lord. Matches JHora's "Nava Taras" for 1_Ramakrishnan.
/// </summary>
public static class NavaTara
{
    private static readonly (string Name, string Meaning)[] Taras =
    {
        ("Janma", "Birth"), ("Sampat", "Wealth"), ("Vipat", "Danger"), ("Kṣema", "Well-being"),
        ("Pratyak", "Obstacles"), ("Sādhana", "Achievement"), ("Naidhana", "Death"),
        ("Mitra", "Friend"), ("Parama Mitra", "Good friend"),
    };

    public static IReadOnlyList<TaraRow> From(ConstellationName reference) =>
        Taras.Select((t, i) =>
        {
            var naks = Enumerable.Range(0, 3).Select(k => (ConstellationName)(((int)reference + i + 9 * k) % 27)).ToList();
            return new TaraRow(i + 1, t.Name, t.Meaning, AstroMath.NakshatraLordOrder[(int)naks[0] % 9], naks);
        }).ToList();

    public static IReadOnlyList<TaraRow> FromLongitude(double longitude) =>
        From(AstroMath.GetNakshatraAndPada(longitude).Nakshatra);
}
