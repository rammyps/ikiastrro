using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Matching;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

/// <summary>Synastry, family-kind and pair-similarity are sign arithmetic over stored D1/D9 signs; these
/// tests pin the arithmetic on small hand-built charts.</summary>
public class SynastryAndSimilarityTests
{
    private static DoshaChart Chart(ZodiacName lagna, ZodiacName moon, ZodiacName venus, ZodiacName mars, ZodiacName rest = ZodiacName.Leo) =>
        new(lagna, new Dictionary<PlanetName, ZodiacName>
        {
            [PlanetName.Sun] = rest, [PlanetName.Moon] = moon, [PlanetName.Mars] = mars, [PlanetName.Mercury] = rest,
            [PlanetName.Jupiter] = rest, [PlanetName.Venus] = venus, [PlanetName.Saturn] = rest,
            [PlanetName.Rahu] = rest, [PlanetName.Ketu] = rest,
        });

    [Fact]
    public void Sign_distance_counts_inclusively_and_wraps()
    {
        Assert.Equal(1, SynastryCalculator.Distance(ZodiacName.Aries, ZodiacName.Aries).Count);
        Assert.Equal(7, SynastryCalculator.Distance(ZodiacName.Aries, ZodiacName.Libra).Count);
        Assert.Equal(12, SynastryCalculator.Distance(ZodiacName.Taurus, ZodiacName.Aries).Count);
        Assert.Equal(2, SynastryCalculator.Distance(ZodiacName.Pisces, ZodiacName.Aries).Count);
    }

    [Fact]
    public void Overlay_puts_each_persons_planets_in_the_others_houses_and_is_directional()
    {
        // First: Lagna Aries, Moon Cancer, Venus Taurus, Mars Aries. Second: Lagna Libra, Moon Aries, Venus Scorpio, Mars Capricornus.
        var first = Chart(ZodiacName.Aries, ZodiacName.Cancer, ZodiacName.Taurus, ZodiacName.Aries);
        var second = Chart(ZodiacName.Libra, ZodiacName.Aries, ZodiacName.Scorpio, ZodiacName.Capricornus);
        var r = SynastryCalculator.Read(first, second);

        // First's Moon (Cancer) from the second's Libra Lagna is the 10th; second's Moon (Aries) from the first's Aries Lagna is the 1st.
        Assert.Equal(10, r.FirstInSecond.Planets.Single(p => p.Planet == PlanetName.Moon).HouseInOther);
        Assert.Equal(1, r.SecondInFirst.Planets.Single(p => p.Planet == PlanetName.Moon).HouseInOther);
        Assert.Equal(7, r.FirstInSecond.LagnaHouseInOther);   // Aries from Libra
        Assert.Equal(7, r.SecondInFirst.LagnaHouseInOther);   // Libra from Aries
        Assert.True(r.FirstInSecond.Planets.Single(p => p.Planet == PlanetName.Mars).Malefic);
        Assert.False(r.FirstInSecond.Planets.Single(p => p.Planet == PlanetName.Venus).Malefic);
    }

    [Fact]
    public void Moon_to_Moon_and_Venus_to_Mars_are_counted_each_way()
    {
        var first = Chart(ZodiacName.Aries, ZodiacName.Cancer, ZodiacName.Taurus, ZodiacName.Aries);
        var second = Chart(ZodiacName.Libra, ZodiacName.Aries, ZodiacName.Scorpio, ZodiacName.Capricornus);
        var r = SynastryCalculator.Read(first, second);

        Assert.Equal(10, r.MoonFirstToSecond.Count);   // Cancer to Aries
        Assert.Equal(4, r.MoonSecondToFirst.Count);    // Aries to Cancer
        Assert.Equal(9, r.VenusFirstToMarsSecond.Count); // Taurus to Capricornus
        Assert.Equal(6, r.VenusSecondToMarsFirst.Count); // Scorpio to Aries
        Assert.False(r.VenusSecondToMarsFirst.IsOpposite);
    }

    [Fact]
    public void Contacts_list_same_sign_and_opposite_sign_pairs_only()
    {
        var first = Chart(ZodiacName.Aries, ZodiacName.Cancer, ZodiacName.Taurus, ZodiacName.Aries, ZodiacName.Gemini);
        var second = Chart(ZodiacName.Libra, ZodiacName.Aries, ZodiacName.Scorpio, ZodiacName.Capricornus, ZodiacName.Virgo);
        var r = SynastryCalculator.Read(first, second);

        // First's Moon (Cancer) opposes second's Mars (Capricornus); first's Mars (Aries) shares a sign with second's Moon (Aries).
        Assert.Contains(new Contact(PlanetName.Moon, PlanetName.Mars, ContactKind.Opposition), r.Contacts);
        Assert.Contains(new Contact(PlanetName.Mars, PlanetName.Moon, ContactKind.SameSign), r.Contacts);
        // First's Venus (Taurus) and second's Venus (Scorpio) are opposite signs; first's Sun (Gemini) and second's Sun (Virgo) are neither.
        Assert.Contains(new Contact(PlanetName.Venus, PlanetName.Venus, ContactKind.Opposition), r.Contacts);
        Assert.DoesNotContain(r.Contacts, c => c.First == PlanetName.Sun && c.Second == PlanetName.Sun);
    }

    [Theory]
    [InlineData("Wife", FamilyKind.Spouse)]
    [InlineData("Husband", FamilyKind.Spouse)]
    [InlineData("Father", FamilyKind.Parent)]
    [InlineData("Daughter", FamilyKind.Child)]
    [InlineData("Brother", FamilyKind.Sibling)]
    [InlineData("Paternal Grandfather", FamilyKind.Grandparent)]
    [InlineData("Maternal Grandmother", FamilyKind.Grandparent)]
    [InlineData("Grandparent", FamilyKind.Grandparent)]
    [InlineData("Granddaughter", FamilyKind.Grandchild)]
    [InlineData("Cousin", FamilyKind.Other)]
    public void Family_roles_map_to_kinds(string role, FamilyKind kind) => Assert.Equal(kind, FamilyRelation.FromRole(role));

    [Theory]
    [InlineData("Wife", FamilyLayer.Core)]
    [InlineData("Father", FamilyLayer.Core)]
    [InlineData("Daughter", FamilyLayer.Core)]
    [InlineData("Paternal Grandmother", FamilyLayer.Extended)]
    [InlineData("Maternal Grandfather", FamilyLayer.Extended)]
    [InlineData("Grandson", FamilyLayer.Extended)]
    [InlineData("Sister", FamilyLayer.Lateral)]
    [InlineData("Cousin", FamilyLayer.Lateral)]
    public void Layers_are_core_extended_and_lateral(string role, FamilyLayer layer) =>
        Assert.Equal(layer, FamilyRelation.LayerOf(FamilyRelation.FromRole(role)));

    [Fact]
    public void A_relationship_turns_around_and_is_labelled_by_sex()
    {
        Assert.Equal(FamilyKind.Parent, FamilyRelation.Invert(FamilyKind.Child));
        Assert.Equal(FamilyKind.Sibling, FamilyRelation.Invert(FamilyKind.Sibling));
        Assert.Equal(FamilyKind.Grandchild, FamilyRelation.Invert(FamilyKind.Grandparent));
        Assert.Equal("Grandson", FamilyRelation.Label(FamilyKind.Grandchild, "Male"));
        Assert.Equal("Father", FamilyRelation.Label(FamilyKind.Parent, "Male"));
        Assert.Equal("Daughter", FamilyRelation.Label(FamilyKind.Child, "Female"));
        Assert.Equal("Sibling", FamilyRelation.Label(FamilyKind.Sibling, null));
    }

    [Fact]
    public void Key_houses_follow_the_other_persons_role()
    {
        Assert.Equal([1, 5, 7, 8, 12], FamilyRelation.KeyHouses(FamilyKind.Spouse));
        Assert.Equal([1, 5], FamilyRelation.KeyHouses(FamilyKind.Child));
        Assert.Equal([1, 4, 9], FamilyRelation.KeyHouses(FamilyKind.Parent));
        Assert.Equal([1, 3, 11], FamilyRelation.KeyHouses(FamilyKind.Sibling));
        Assert.Equal([1], FamilyRelation.KeyHouses(FamilyKind.Grandparent));
    }

    private static SimilarityInput Input(DoshaChart d1, int nak = 3, string gana = "Deva", string yoni = "Horse", string nadi = "Vata", int pada = 1) =>
        new(d1, new MatchPerson(d1.Signs[PlanetName.Moon], nak, gana, yoni, "Male", nadi), pada, null);

    [Fact]
    public void Similarity_lists_only_what_is_shared()
    {
        var a = Input(Chart(ZodiacName.Aries, ZodiacName.Cancer, ZodiacName.Taurus, ZodiacName.Aries, ZodiacName.Gemini));
        var b = Input(Chart(ZodiacName.Aries, ZodiacName.Cancer, ZodiacName.Pisces, ZodiacName.Leo, ZodiacName.Gemini), yoni: "Dog", pada: 2);
        var shared = ChartSimilarity.Compare(a, b, "A", "B");

        Assert.Contains(shared, f => f.Category == "Lagna");
        Assert.Contains(shared, f => f.Category == "Moon");
        Assert.Contains(shared, f => f.Text == "Both have the same Moon nakshatra.");
        Assert.DoesNotContain(shared, f => f.Text.Contains("pada"));            // padas differ
        Assert.DoesNotContain(shared, f => f.Text.Contains("Yoni"));            // Horse vs Dog
        Assert.Contains(shared, f => f.Text == "Jupiter is in Gemini in both.");
        Assert.DoesNotContain(shared, f => f.Text.StartsWith("Venus"));          // Taurus vs Pisces
    }

    [Fact]
    public void Similarity_notes_when_one_lagna_is_the_others_moon_sign()
    {
        var a = Input(Chart(ZodiacName.Aries, ZodiacName.Cancer, ZodiacName.Taurus, ZodiacName.Aries), nak: 1);
        var b = Input(Chart(ZodiacName.Libra, ZodiacName.Aries, ZodiacName.Taurus, ZodiacName.Aries), nak: 2);
        var shared = ChartSimilarity.Compare(a, b, "Asha", "Bala");

        Assert.Contains(shared, f => f.Category == "Cross" && f.Text.StartsWith("Bala's Lagna sign (Libra)") == false && f.Text.StartsWith("Asha's Lagna sign (Aries) is Bala's Moon sign"));
    }
}
