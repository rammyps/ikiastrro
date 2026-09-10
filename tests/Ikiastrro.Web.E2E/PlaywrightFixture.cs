using System.Net.Http;
using Microsoft.Playwright;
using Xunit;

namespace Ikiastrro.Web.E2E;

/// <summary>
/// One Chromium instance for the whole test run, plus a one-time probe of the target app.
/// Nothing here launches the .NET app — run it yourself on <see cref="BaseUrl"/> first.
/// </summary>
public sealed class PlaywrightFixture : IAsyncLifetime
{
    public string BaseUrl { get; } =
        Environment.GetEnvironmentVariable("IKIASTRRO_E2E_BASEURL")?.TrimEnd('/')
        ?? "http://localhost:5160";

    /// <summary>True when something answered on <see cref="BaseUrl"/> during setup.</summary>
    public bool AppIsUp { get; private set; }

    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    private static bool Headed =>
        Environment.GetEnvironmentVariable("IKIASTRRO_E2E_HEADED") is "1" or "true";

    public async Task InitializeAsync()
    {
        using (var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) })
        {
            try
            {
                var res = await http.GetAsync(BaseUrl + "/");
                AppIsUp = res.IsSuccessStatusCode;
            }
            catch
            {
                AppIsUp = false;
            }
        }

        if (!AppIsUp)
            return; // tests Skip.IfNot — no point paying for a browser launch

        Microsoft.Playwright.Program.Main(["install", "chromium"]); // no-op once cached
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new()
        {
            Headless = !Headed,
            SlowMo = Headed ? 250 : 0,
        });
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
            await Browser.DisposeAsync();
        Playwright?.Dispose();
    }
}

[CollectionDefinition(PlaywrightCollection.Name)]
public sealed class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
    public const string Name = "playwright";
}
