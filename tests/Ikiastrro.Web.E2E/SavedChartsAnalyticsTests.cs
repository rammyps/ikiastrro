using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace Ikiastrro.Web.E2E;

[Collection(PlaywrightCollection.Name)]
public sealed class SavedChartsAnalyticsTests(PlaywrightFixture fixture) : E2ETestBase(fixture)
{
    [SkippableFact]
    public async Task Eligibility_requires_visible_explicit_permission_confirmation()
    {
        Skip.IfNot(Fixture.AppIsUp, $"No app on {Fixture.BaseUrl}");
        await OpenAsync("/charts");

        var status = Page.Locator(".sp-research-status").First;
        await Expect(status).ToHaveTextAsync("Not enrolled");
        await status.ClickAsync();

        var dialog = Page.Locator(".research-box");
        await Expect(dialog).ToBeVisibleAsync();
        await dialog.Locator("select").Nth(0).SelectOptionAsync("ELIGIBLE");
        await dialog.Locator("select").Nth(1).SelectOptionAsync("RESEARCH");
        await Expect(dialog.GetByText("I confirm that explicit permission")).ToBeVisibleAsync();

        await dialog.GetByRole(AriaRole.Button, new() { Name = "Save status" }).ClickAsync();
        await Expect(dialog.GetByRole(AriaRole.Alert))
            .ToContainTextAsync("explicit recorded permission");
        await Expect(dialog).ToBeVisibleAsync();

        AssertCleanConsole();
    }
}
