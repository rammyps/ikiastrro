using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Playwright.Assertions;

namespace Ikiastrro.Web.E2E;

/// <summary>
/// The full person lifecycle from the UI: add via Home → chart generates and renders →
/// delete from /charts. Creates real rows + chart data, so it always removes the test
/// person at the end (and defends against a leftover from a prior crashed run at the start).
/// </summary>
[Collection(PlaywrightCollection.Name)]
public sealed class GenerateChartTests(PlaywrightFixture fixture, ITestOutputHelper output)
    : E2ETestBase(fixture)
{
    private static readonly System.Text.RegularExpressions.Regex TransitUrl = new(@"/transit-wheel/\d+");
    private const string Person = "E2E GenTest Person";

    [SkippableFact]
    public async Task Add_generates_a_rendering_chart_then_delete_removes_it_cleanly()
    {
        Skip.IfNot(Fixture.AppIsUp, $"No app on {Fixture.BaseUrl}");

        await ForceDeleteTestPerson();

        // ---- add + generate ---------------------------------------------------
        await OpenAsync("/");
        await Page.GetByPlaceholder("Search saved people…").FillAsync(Person);
        var form = Page.Locator(".home-form");
        await Expect(form).ToBeVisibleAsync();

        await form.GetByRole(AriaRole.Radio, new() { Name = "Male" }).First.CheckAsync();
        await form.Locator("div.mud-input-control:has(label:has-text('Date of Birth')) input")
            .First.FillAsync("22 Apr 1981");
        await form.Locator("div.mud-input-control:has(label:has-text('Time of Birth')) input")
            .First.FillAsync("05:30 AM");
        await form.GetByLabel("City").First.FillAsync("Chennai");
        await form.GetByLabel("Country").First.FillAsync("India");
        await form.GetByRole(AriaRole.Button, new() { Name = "Generate Chart" }).ClickAsync();

        try
        {
            await Page.WaitForURLAsync(TransitUrl, new() { Timeout = 45_000 });
        }
        catch (TimeoutException)
        {
            var err = await Page.Locator(".home-form-error").TextContentAsync();
            Assert.Fail($"Generate did not navigate to the transit page. Inline error: \"{err?.Trim()}\"");
        }

        // ---- the generated chart must actually have data ---------------------
        await Expect(Page.GetByText("Generate a D1 chart for this person first")).Not.ToBeVisibleAsync();
        await Expect(Page.Locator(".tw-table tbody tr").First).ToBeVisibleAsync(new() { Timeout = 15_000 });
        var d1Rows = await Page.Locator(".tw-table tbody tr").CountAsync();
        output.WriteLine($"D1 Birth table rows after generate: {d1Rows}");
        Assert.True(d1Rows >= 8, $"expected >= 8 D1 rows on the transit page, got {d1Rows}");

        // ---- delete from /charts -------------------------------------------------
        await OpenAsync("/charts");
        var row = Page.Locator("tr", new() { HasTextString = Person });
        await Expect(row).ToHaveCountAsync(1);
        await row.First.GetByRole(AriaRole.Button, new() { Name = $"Delete {Person}" }).ClickAsync();
        await Page.Locator(".dialog-box button.btn-confirm").ClickAsync();

        await Expect(Page.Locator("tr", new() { HasTextString = Person })).ToHaveCountAsync(0, new() { Timeout = 20_000 });
        // a delete that throws would blow up the whole circuit
        await Expect(Page.Locator("#blazor-error-ui")).Not.ToBeVisibleAsync();
        await Expect(Page.Locator(".saved-charts-error")).Not.ToBeVisibleAsync();

        AssertCleanConsole();
    }

    /// <summary>
    /// Best-effort pre-clean. The delete bug that used to crash the circuit is fixed, but keep
    /// this resilient so one bad run doesn't wedge every subsequent run on the unique-name index.
    /// </summary>
    private async Task ForceDeleteTestPerson()
    {
        await OpenAsync("/charts");
        var row = Page.Locator("tr", new() { HasTextString = Person });
        if (await row.CountAsync() == 0)
            return;
        try
        {
            await row.First.GetByRole(AriaRole.Button, new() { Name = $"Delete {Person}" }).ClickAsync();
            await Page.Locator(".dialog-box button.btn-confirm").ClickAsync();
            await Expect(Page.Locator("tr", new() { HasTextString = Person })).ToHaveCountAsync(0, new() { Timeout = 20_000 });
        }
        catch (Exception ex)
        {
            output.WriteLine($"[pre-clean] could not delete leftover '{Person}': {ex.Message}");
        }
    }
}
