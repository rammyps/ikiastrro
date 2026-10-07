using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dignity;
using Ikiastrro.Core.Engines.PlanetaryStates;
using Ikiastrro.Core.Engines.Relationships;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

/// <summary>
/// Dīptādi + (Lajjita, Garvita) against Jagannatha Hora's "Mood (D-1)" column for the four
/// reference natal exports in docs/artifacts/reference-charts (Rammy = 1_Ramakrishnan, Ananya,
/// Ramya, Sundari). Positions are transcribed from each export's own body table. JHora prints only
/// Lajjita and Garvita from PVR's second six, so only those two are compared. No DB: a hand-built
/// rule set stands in for the repository.
/// Rahu/Ketu: the relationship tier (Mudita/Saanta/Deena/Duhkhita) is not compared — JHora gives the
/// nodes real friend/enemy tiers and the engine has no node Naisargika table (open, decision 010).
/// </summary>
public class AvasthaJhoraMoodGoldenTests
{
    private static readonly string[] Tiers = { "Mudita", "Saanta", "Deena", "Duhkhita" };
    private static readonly string[] Order = { "Sun", "Moon", "Mars", "Mercury", "Jupiter", "Venus", "Saturn", "Rahu", "Ketu" };

    private static readonly PlanetaryStateRuleSet Rules = BuildRules();

    private static PlanetaryStateRuleSet BuildRules()
    {
        byte id = 1;
        PlanetaryStateRow Row(string system, string name) => new(id++, system, name, 1, null);
        var dee = new[] { "Deepta", "Swastha", "Mudita", "Saanta", "Deena", "Duhkhita", "Vikala", "Khala", "Kopita" }
            .Select(n => Row("Deeptadi", n)).ToDictionary(r => r.StateName);
        var laj = new[] { "Lajjita", "Garvita", "Kshudhita", "Trishita", "Mudita", "Kshobhita" }
            .Select(n => Row("Lajjitadi", n)).ToDictionary(r => r.StateName);

        byte rid = 1;
        DeeptadiStateRuleRow Rule(string dignity, string state) => new(rid++, 1, dignity, dee[state].Id, state);
        var byDignity = new[]
        {
            Rule("Exalted", "Deepta"), Rule("Moolatrikona", "Swastha"), Rule("Own Sign", "Swastha"),
            Rule("Great Friend", "Mudita"), Rule("Friend", "Saanta"), Rule("Neutral", "Deena"),
            Rule("Enemy", "Duhkhita"), Rule("Great Enemy", "Duhkhita"), Rule("Debilitated", "Duhkhita"),
        }.ToDictionary(r => r.DignityStatus);

        return new PlanetaryStateRuleSet(1, new List<AgeStateRuleRow>(),
            new Dictionary<string, WakefulnessStateRuleRow>(), new Dictionary<byte, PlanetaryStateRow>(),
            byDignity, dee, laj);
    }

    /// <summary>One graha as the JHora export prints it: sign, degrees within the sign, mood states.</summary>
    public record Cell(ZodiacName Sign, double Degrees, string Expected);

    private const ZodiacName Ar = ZodiacName.Aries, Ta = ZodiacName.Taurus, Ge = ZodiacName.Gemini, Cn = ZodiacName.Cancer,
        Vi = ZodiacName.Virgo, Li = ZodiacName.Libra, Sc = ZodiacName.Scorpio, Sg = ZodiacName.Sagittarius,
        Cp = ZodiacName.Capricornus, Pi = ZodiacName.Pisces;

    /// <summary>Planet order in each array: Sun, Moon, Mars, Mercury, Jupiter, Venus, Saturn, Rahu, Ketu.</summary>
    private static readonly Dictionary<string, (ZodiacName Lagna, Cell[] Cells)> Reference = new()
    {
        ["Rammy"] = (Ar, new[]
        {
            new Cell(Ar,  8.205, "Deepta,Deena,Khala,Garvita"),
            new Cell(Sc,  7.293, "Duhkhita,Khala"),
            new Cell(Ar,  3.950, "Swastha,Khala,Kopita,Garvita"),
            new Cell(Ar,  1.843, "Duhkhita,Vikala,Khala"),
            new Cell(Vi,  8.727, "Duhkhita"),
            new Cell(Ar, 11.992, "Duhkhita,Vikala,Khala,Kopita"),
            new Cell(Vi, 10.957, "Deena"),
            new Cell(Cn, 13.059, "Duhkhita"),
            new Cell(Cp, 13.059, "Deena,Khala"),
        }),
        ["Ananya"] = (Cn, new[]
        {
            new Cell(Sc, 28.525, "Mudita,Khala"),
            new Cell(Pi,  9.938, "Duhkhita"),
            new Cell(Sg, 11.049, "Mudita"),
            new Cell(Sg, 10.351, "Saanta,Vikala"),
            new Cell(Pi,  0.633, "Swastha"),
            new Cell(Li, 14.582, "Swastha,Garvita"),
            new Cell(Vi, 21.659, "Mudita"),
            new Cell(Sg,  9.228, "Saanta"),
            new Cell(Ge,  9.228, "Duhkhita"),
        }),
        ["Sundari"] = (Li, new[]
        {
            new Cell(Ta, 17.085, "Deena"),
            new Cell(Sc, 10.084, "Duhkhita,Khala"),
            new Cell(Pi,  2.496, "Deena"),
            new Cell(Ar, 28.489, "Saanta,Khala"),
            new Cell(Vi, 28.961, "Duhkhita"),
            new Cell(Ar,  6.951, "Saanta,Khala"),
            new Cell(Sg,  0.072, "Saanta"),
            new Cell(Li,  6.107, "Deena"),
            new Cell(Ar,  6.107, "Deena,Khala"),
        }),
        ["Ramya"] = (Ge, new[]
        {
            new Cell(Sc, 23.332, "Mudita,Khala"),
            new Cell(Cp, 21.261, "Saanta,Khala"),
            new Cell(Vi, 18.661, "Deena"),
            new Cell(Sg, 13.269, "Saanta"),
            new Cell(Sc, 27.232, "Mudita,Vikala,Khala,Kopita"),
            new Cell(Li,  9.773, "Swastha,Lajjita,Garvita"),
            new Cell(Li, 18.186, "Deepta,Deena,Garvita"),
            new Cell(Ta, 22.102, "Deena"),
            new Cell(Sc, 22.102, "Swastha,Deena,Khala,Kopita"),
        }),
    };

    [Theory]
    [InlineData("Rammy")]
    [InlineData("Ananya")]
    [InlineData("Sundari")]
    [InlineData("Ramya")]
    public void Mood_column_matches_jhora(string chart)
    {
        var (lagna, cells) = Reference[chart];
        var signs = Order.Select((p, i) => (p, cells[i].Sign)).ToDictionary(x => x.p, x => x.Sign);
        var longitude = Order.Select((p, i) => (p, (int)cells[i].Sign * 30 + cells[i].Degrees)).ToDictionary(x => x.p, x => x.Item2);

        for (var i = 0; i < Order.Length; i++)
        {
            var planet = Order[i];
            var cell = cells[i];
            var dignity = DignityEngine.Evaluate(planet, cell.Sign, cell.Degrees, signs);
            var conjunct = Order.Where(o => o != planet && signs[o] == cell.Sign).ToList();

            string? relation = null;
            if (dignity.SignLordPlanet is { } lord && lord != planet && signs.TryGetValue(lord, out var lordSign))
                relation = DignityEngine.EvaluatePairRelationship(planet, lord, cell.Sign, lordSign);

            decimal? sunDistance = planet == "Sun"
                ? null
                : CombustionEngine.AngularSeparation(longitude[planet], longitude["Sun"]);

            var dee = DeeptadiStateCalculator.For(
                dignity.DignityStatus, relation, dignity.SignLordPlanet, conjunct, sunDistance, Rules);
            var actual = dee.Select(x => Rules.DeeptadiStatesByName.Values.Single(r => r.Id == x).StateName).ToList();

            var house = ((int)cell.Sign - (int)lagna + 12) % 12 + 1;
            var laj = LajjitadiStateCalculator.For(
                cell.Sign.ToString(), dignity.DignityStatus, house, conjunct, Array.Empty<string>(),
                ClassicalMalefics.Contains, _ => null, Rules);
            actual.AddRange(laj.Select(x => Rules.LajjitadiStatesByName.Values.Single(r => r.Id == x).StateName)
                .Where(n => n is "Lajjita" or "Garvita"));

            var expected = cell.Expected.Split(',').ToList();
            if (planet is "Rahu" or "Ketu")
            {
                actual.RemoveAll(Tiers.Contains);
                expected.RemoveAll(Tiers.Contains);
            }

            Assert.True(expected.OrderBy(x => x).SequenceEqual(actual.OrderBy(x => x)),
                $"{chart} {planet}: JHora [{string.Join(",", expected.OrderBy(x => x))}] vs ours [{string.Join(",", actual.OrderBy(x => x))}]");
        }
    }
}
