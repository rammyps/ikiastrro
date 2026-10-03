using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace Ikiastrro.Web.E2E;

[Collection(PlaywrightCollection.Name)]
public sealed class KeyInferencePopulationTests(PlaywrightFixture fixture) : E2ETestBase(fixture)
{
    [SkippableFact]
    public async Task Unenrolled_chart_shows_guarded_population_state()
    {
        Skip.IfNot(Fixture.AppIsUp, $"No app on {Fixture.BaseUrl}");
        await OpenAsync("/key-inference/1");

        var card = Page.Locator(".lm-population");
        await Expect(card).ToBeVisibleAsync();
        await Expect(card).ToContainTextAsync("Population comparison unavailable");
        await Expect(card).ToContainTextAsync("not enrolled for statistical use");
        await Expect(card).ToContainTextAsync("not an outcome probability");
        await Expect(card).Not.ToContainTextAsync("0th percentile");

        AssertCleanConsole();
    }
}
