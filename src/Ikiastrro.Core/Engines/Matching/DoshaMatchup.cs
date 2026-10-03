using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Matching;

/// <summary>One kind of affliction and where each person has it, as "8th from Lagna" strings.</summary>
public sealed record DoshaMatchupRow(string Dosha, IReadOnlyList<string> First, IReadOnlyList<string> Second)
{
    /// <summary>True when both people have this affliction, or neither does.</summary>
    public bool SameInBoth => (First.Count > 0) == (Second.Count > 0);
}

/// <summary>
/// The dosha comparison laid out by kind of affliction, so it is plain whether both charts carry the same
/// one: Mars or Rahu in the 7th or 8th (the book's severe pair, p.54), Rahu in the 4th, Mars or Rahu in the
/// lesser houses, and Mars or Saturn in the 1st. This only sorts what <see cref="DoshaSamyaCalculator"/>
/// already found; the single balancing rule the book states (pp.56-58) stays in
/// <see cref="DoshaSamyaCalculator.Balance"/>. Cancellations by the afflicting planet's dignity, by benefic
/// aspects or by its dispositor are not applied: no cited source gives them. Pure; no I/O.
/// </summary>
public static class DoshaMatchup
{
    public static IReadOnlyList<DoshaMatchupRow> Build(DoshaReading first, DoshaReading second)
    {
        var kinds = new (string Name, Func<DoshaAffliction, bool> Match)[]
        {
            ("Mars in the 7th or 8th", a => a.Malefic == PlanetName.Mars && a.House is 7 or 8),
            ("Rahu in the 7th or 8th", a => a.Malefic == PlanetName.Rahu && a.House is 7 or 8),
            ("Rahu in the 4th", a => a.Malefic == PlanetName.Rahu && a.House == 4),
            ("Mars in the 2nd, 4th or 12th", a => a.Malefic == PlanetName.Mars && a.House is 2 or 4 or 12),
            ("Rahu in the 2nd or 12th", a => a.Malefic == PlanetName.Rahu && a.House is 2 or 12),
            ("Mars or Saturn in the 1st", a => a.Malefic is PlanetName.Mars or PlanetName.Saturn && a.House == 1),
            ("Sun, Saturn or Ketu in the 2nd, 4th, 7th, 8th or 12th", a => a.Malefic is PlanetName.Sun or PlanetName.Saturn or PlanetName.Ketu && a.House != 1),
        };
        return kinds.Select(k => new DoshaMatchupRow(k.Name, Where(first, k.Match), Where(second, k.Match))).ToList();
    }

    private static List<string> Where(DoshaReading reading, Func<DoshaAffliction, bool> match) =>
        reading.Afflictions.Where(match).Select(a => $"{Ordinal(a.House)} from {a.From}").ToList();

    private static string Ordinal(int n) => n switch { 1 => "1st", 2 => "2nd", 3 => "3rd", _ => $"{n}th" };
}
