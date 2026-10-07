namespace Ikiastrro.Core.Engines.PlanetaryStates;

/// <summary>
/// The fixed natural-malefic set the Dīptādi states (Vikala, Khala) use: Sun, Mars, Saturn, Rahu,
/// Ketu. Unlike <c>ArgalaCalculator.IsNaturalMalefic</c> it never makes the Moon or Mercury
/// conditional — Jagannatha Hora's Mood column is reproduced only with this static reading
/// (Mercury and the waning Moon never count as malefic sign lords or companions).
/// </summary>
public static class ClassicalMalefics
{
    private static readonly HashSet<string> Set = new() { "Sun", "Mars", "Saturn", "Rahu", "Ketu" };

    public static bool Contains(string planet) => Set.Contains(planet);
}
