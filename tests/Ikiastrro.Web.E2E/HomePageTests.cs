using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace Ikiastrro.Web.E2E;

/// <summary>
/// Regression coverage for the Home screen fixes (commit 99b19cc): the four bugs here were
/// invisible to bUnit because they only manifest in a real interactive circuit with JS +
/// MudBlazor popovers.
/// </summary>
[Collection(PlaywrightCollection.Name)]
public sealed class HomePageTests(PlaywrightFixture fixture) : E2ETestBase(fixture)
{
    private ILocator PrefsToggle => Page.Locator(".home-prefs-toggle");
    private ILocator SearchBox => Page.GetByPlaceholder("Search saved people…");
    private ILocator AddForm => Page.Locator(".home-form");

    [SkippableFact]
    public async Task Home_loads_with_no_console_errors()
    {
        Skip.IfNot(Fixture.AppIsUp, $"No app on {Fixture.BaseUrl} — start src/Ikiastrro.Web first.");

        await OpenAsync("/");
        await Expect(Page.Locator("h1.home-title")).ToHaveTextAsync("Discover Your Path");

        AssertCleanConsole();
    }

    [SkippableFact]
    public async Task Preferences_dropdowns_open_and_list_their_options()
    {
        Skip.IfNot(Fixture.AppIsUp, $"No app on {Fixture.BaseUrl}");
        await OpenAsync("/");

        await PrefsToggle.ClickAsync();

        // Ayanamsa — the full JHora catalogue (21+ systems), Lahiri present.
        var ayanamsa = Page.GetByRole(AriaRole.Combobox, new() { Name = "Choose Ayanāṁśa" });
        await ayanamsa.ClickAsync();
        var options = Page.Locator(".mud-popover-open .mud-list-item");
        await Expect(options.First).ToBeVisibleAsync();
        Assert.True(await options.CountAsync() >= 20, $"expected >= 20 ayanamsa options, got {await options.CountAsync()}");
        await Expect(Page.Locator(".mud-popover-open .mud-list-item", new() { HasTextString = "Lahiri" }).First)
            .ToBeVisibleAsync();
        await Page.Keyboard.PressAsync("Escape");

        // Chart type — South selectable; North / West present but disabled (no renderer yet).
        var chartType = Page.GetByRole(AriaRole.Combobox, new() { Name = "Choose Chart Type" });
        await chartType.ClickAsync();
        await Expect(Page.Locator(".mud-popover-open .mud-list-item", new() { HasTextString = "South Indian" }))
            .ToBeVisibleAsync();
        var north = Page.Locator(".mud-popover-open .mud-list-item", new() { HasTextString = "North Indian" });
        await Expect(north).ToHaveAttributeAsync("aria-disabled", "true");

        AssertCleanConsole();
    }

    [SkippableFact]
    public async Task An_unknown_name_reveals_the_prefilled_Add_form()
    {
        Skip.IfNot(Fixture.AppIsUp, $"No app on {Fixture.BaseUrl}");
        await OpenAsync("/");

        await SearchBox.FillAsync("Zzz Nobody E2E");

        await Expect(AddForm).ToBeVisibleAsync();
        await Expect(Page.Locator(".home-form-caption")).ToHaveTextAsync("New person");
        await Expect(AddForm.Locator("input[type='text']").First).ToHaveValueAsync("Zzz Nobody E2E");
        await Expect(SearchBox).ToBeVisibleAsync(); // search stays put

        // clearing the search retracts the still-pristine form
        await SearchBox.FillAsync("");
        await Expect(AddForm).Not.ToBeVisibleAsync();

        AssertCleanConsole();
    }

    [SkippableFact]
    public async Task The_birth_date_picker_opens_to_year_view_and_accepts_a_1981_date()
    {
        Skip.IfNot(Fixture.AppIsUp, $"No app on {Fixture.BaseUrl}");
        await OpenAsync("/");
        await SearchBox.FillAsync("Zzz Nobody E2E"); // auto-opens the form

        var dob = AddForm.Locator("div.mud-input-control:has(label:has-text('Date of Birth'))");
        var dobInput = dob.Locator("input[type='text']");

        // editable: MudDatePicker drops `readonly` when Editable="true"
        Assert.Null(await dobInput.GetAttributeAsync("readonly"));

        await dob.GetByRole(AriaRole.Button, new() { Name = "Open" }).ClickAsync();

        // OpenTo="Year" → a long year list, not a month grid
        var years = Page.Locator(".mud-picker-open .mud-picker-year");
        await Expect(years.First).ToBeVisibleAsync();
        Assert.True(await years.CountAsync() > 100);

        await Page.Locator(".mud-picker-open .mud-picker-year", new() { HasTextString = "1981" }).ClickAsync();
        await Page.Locator(".mud-picker-open .mud-picker-month", new() { HasTextString = "Apr" }).ClickAsync();
        await Page.Locator(".mud-picker-open button.mud-picker-calendar-day", new() { HasTextString = "22" })
            .First.ClickAsync();

        await Expect(dobInput).ToHaveValueAsync("22 Apr 1981");
        AssertCleanConsole();
    }

    [SkippableFact]
    public async Task The_Ganesha_art_is_large_and_hugs_the_right_edge()
    {
        Skip.IfNot(Fixture.AppIsUp, $"No app on {Fixture.BaseUrl}");
        await OpenAsync("/");

        var art = Page.Locator(".home-art img");
        await Expect(art).ToBeVisibleAsync();
        var box = await art.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.True(box!.Width >= 560, $"art width {box.Width} < 560 — not enlarged");

        var viewport = Page.ViewportSize!;
        Assert.True(box.X + box.Width >= viewport.Width - 90,
            $"art right edge {box.X + box.Width:F0} is not near the {viewport.Width}px content edge");

        // no horizontal page scroll introduced by the wider column
        var overflow = await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollWidth > document.documentElement.clientWidth + 1");
        Assert.False(overflow, "home page scrolls horizontally");

        AssertCleanConsole();
    }
}
