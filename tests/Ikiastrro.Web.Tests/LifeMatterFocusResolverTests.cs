using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.LifeMatters;

namespace Ikiastrro.Web.Tests;

public sealed class LifeMatterFocusResolverTests
{
    private readonly LifeMatterFocusResolver _resolver = new();

    [Fact]
    public void Resolve_CombinesIndependentSourcesInStableOrder()
    {
        var result = _resolver.Resolve(
            2,
            42,
            [new(1, 2, 42, "MARRIAGE_RELATIONSHIPS", "D9")],
            [new(2, 2, 42, 8, "KARAKA_DK", 2), new(1, 2, 42, 9, "GRAHA_VENUS", 1)],
            [new(2, 2, 42, LifeMatterFocusKind.SpecialPoint, null, null, "A12", 2),
             new(1, 2, 42, LifeMatterFocusKind.House, "LAGNA", 7, null, 1)]);

        Assert.Equal("D9", result.Subject?.ChartTypeCode);
        Assert.Equal(["GRAHA_VENUS", "KARAKA_DK"], result.Karakas.Select(x => x.KarakaCode));
        Assert.Equal([LifeMatterFocusKind.House, LifeMatterFocusKind.SpecialPoint],
            result.HouseAndSpecialPointFoci.Select(x => x.FocusKind));
        Assert.True(result.IsFocusStructured);
    }

    [Fact]
    public void Resolve_NoKarakaOrFocus_IsExplicitlyUnstructuredEvenWithSubject()
    {
        var result = _resolver.Resolve(
            1,
            9,
            [new(1, 1, 9, "OVERALL_STRENGTH_DHARMA", "D1")],
            [],
            []);

        Assert.NotNull(result.Subject);
        Assert.False(result.IsFocusStructured);
    }

    [Fact]
    public void Resolve_RejectsMoreThanOneActiveSubject()
    {
        var subjects = new[]
        {
            new LifeMatterSubjectRule(1, 1, 7, "ONE", "D1"),
            new LifeMatterSubjectRule(2, 1, 7, "TWO", "D9")
        };

        Assert.Throws<InvalidOperationException>(() => _resolver.Resolve(1, 7, subjects, [], []));
    }

    [Fact]
    public void Resolve_RejectsUlAliasAndRequiresCanonicalA12()
    {
        var foci = new[]
        {
            new LifeMatterFocusRule(1, 1, 7, LifeMatterFocusKind.SpecialPoint, null, null, "UL", 1)
        };

        var error = Assert.Throws<InvalidOperationException>(() => _resolver.Resolve(1, 7, [], [], foci));
        Assert.Contains("A12", error.Message);
    }

    [Theory]
    [InlineData(ZodiacName.Aries, 7, ZodiacName.Libra)]
    [InlineData(ZodiacName.Capricornus, 7, ZodiacName.Cancer)]
    [InlineData(ZodiacName.Scorpio, 9, ZodiacName.Cancer)]
    public void ResolveHouseSign_TranslatesRelativeHouseForDisplayedChart(
        ZodiacName ascendant,
        int house,
        ZodiacName expectedSign)
    {
        Assert.Equal(expectedSign, LifeMatterFocusResolver.ResolveHouseSign(ascendant, house));
    }
}
