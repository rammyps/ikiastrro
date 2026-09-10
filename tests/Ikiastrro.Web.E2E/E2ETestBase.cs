using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;

namespace Ikiastrro.Web.E2E;

/// <summary>
/// Fresh browser context + page per test, with console errors, uncaught page exceptions and
/// failed navigations collected. Call <see cref="OpenAsync"/> to navigate, and
/// <see cref="AssertCleanConsoleAsync"/> before finishing.
/// </summary>
public abstract class E2ETestBase(PlaywrightFixture fixture) : IAsyncLifetime
{
    protected PlaywrightFixture Fixture { get; } = fixture;

    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;

    private readonly List<string> _errors = [];

    // console noise that isn't a real defect (favicon, dev-time SignalR chatter, etc.)
    private static readonly Regex Ignorable = new(
        @"favicon|/_framework/|Failed to load resource: .*\b404\b.*favicon",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task InitializeAsync()
    {
        if (!Fixture.AppIsUp)
            return;

        Context = await Fixture.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1440, Height = 900 },
            BaseURL = Fixture.BaseUrl,
            ColorScheme = ColorScheme.Light,
        });
        Page = await Context.NewPageAsync();

        Page.Console += (_, msg) =>
        {
            if (msg.Type is "error" && !Ignorable.IsMatch(msg.Text))
                _errors.Add($"console.error: {msg.Text}");
        };
        Page.PageError += (_, err) => _errors.Add($"pageerror: {err}");
        Page.Response += (_, res) =>
        {
            if ((int)res.Status >= 500)
                _errors.Add($"HTTP {res.Status} {res.Url}");
        };
    }

    public async Task DisposeAsync()
    {
        if (Context is not null)
            await Context.DisposeAsync();
    }

    /// <summary>Navigate to a route and wait for Blazor to be interactive-idle.</summary>
    protected async Task OpenAsync(string path = "/")
    {
        await Page.GotoAsync(path, new() { WaitUntil = WaitUntilState.NetworkIdle });
    }

    protected void AssertCleanConsole() =>
        Assert.True(_errors.Count == 0,
            "Browser reported problems:\n  " + string.Join("\n  ", _errors));

    /// <summary>Screenshot into reports/e2e/ — handy artifact when a test fails locally.</summary>
    protected async Task SnapshotAsync(string name)
    {
        Directory.CreateDirectory("reports/e2e");
        await Page.ScreenshotAsync(new()
        {
            Path = $"reports/e2e/{name}.png",
            FullPage = true,
        });
    }
}
