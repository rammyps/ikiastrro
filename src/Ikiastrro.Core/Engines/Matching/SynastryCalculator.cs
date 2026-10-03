using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Matching;

/// <summary>One of a person's grahas laid on the other person's chart: its sign, the house that sign is
/// in the other chart (Whole Sign from the other's Lagna), and whether it is one of the five malefics
/// Dosha Samya uses (Sun, Mars, Saturn, Rahu, Ketu).</summary>
public sealed record OverlayRow(PlanetName Planet, ZodiacName Sign, int HouseInOther, bool Malefic);

/// <summary>One person's Lagna and grahas placed in the other's houses.</summary>
public sealed record Overlay(ZodiacName LagnaSign, int LagnaHouseInOther, IReadOnlyList<OverlayRow> Planets);

/// <summary>The count of signs from one sign to another, inclusive: 1 is the same sign, 7 the opposite.</summary>
public readonly record struct SignDistance(int Count)
{
    public bool IsSameSign => Count == 1;
    public bool IsOpposite => Count == 7;
}

public enum ContactKind { SameSign, Opposition }

/// <summary>A planet of the first chart and a planet of the second sharing a sign, or in opposite signs.</summary>
public sealed record Contact(PlanetName First, PlanetName Second, ContactKind Kind);

/// <summary>
/// Cross-chart (synastry) facts for any two saved people, directional and without a verdict: each person's
/// grahas and Lagna in the other's houses, the Moon-to-Moon sign count each way, Venus-to-Mars each way, and
/// every planet pair sharing a sign or standing in opposite signs. Only D1 signs are read, so contacts are
/// by sign, not by degree, and the special aspects (Mars, Jupiter, Saturn) are not computed. No source is
/// cited for what any contact means, so none is given. Pure; no I/O.
/// </summary>
public static class SynastryCalculator
{
    private static readonly PlanetName[] Malefics =
        [PlanetName.Sun, PlanetName.Mars, PlanetName.Saturn, PlanetName.Rahu, PlanetName.Ketu];

    public sealed record Reading(
        Overlay FirstInSecond,
        Overlay SecondInFirst,
        SignDistance MoonFirstToSecond,
        SignDistance MoonSecondToFirst,
        SignDistance VenusFirstToMarsSecond,
        SignDistance VenusSecondToMarsFirst,
        IReadOnlyList<Contact> Contacts);

    public static Reading Read(DoshaChart first, DoshaChart second)
    {
        var contacts = new List<Contact>();
        foreach (var a in Enum.GetValues<PlanetName>())
            foreach (var b in Enum.GetValues<PlanetName>())
            {
                var d = Distance(first.Signs[a], second.Signs[b]);
                if (d.IsSameSign) contacts.Add(new(a, b, ContactKind.SameSign));
                else if (d.IsOpposite) contacts.Add(new(a, b, ContactKind.Opposition));
            }

        return new Reading(
            Lay(first, second), Lay(second, first),
            Distance(first.Signs[PlanetName.Moon], second.Signs[PlanetName.Moon]),
            Distance(second.Signs[PlanetName.Moon], first.Signs[PlanetName.Moon]),
            Distance(first.Signs[PlanetName.Venus], second.Signs[PlanetName.Mars]),
            Distance(second.Signs[PlanetName.Venus], first.Signs[PlanetName.Mars]),
            contacts);
    }

    public static SignDistance Distance(ZodiacName from, ZodiacName to) => new((((int)to - (int)from + 12) % 12) + 1);

    private static Overlay Lay(DoshaChart mine, DoshaChart other) =>
        new(mine.Lagna, Distance(other.Lagna, mine.Lagna).Count,
            Enum.GetValues<PlanetName>()
                .Select(p => new OverlayRow(p, mine.Signs[p], Distance(other.Lagna, mine.Signs[p]).Count, Malefics.Contains(p)))
                .ToList());
}
