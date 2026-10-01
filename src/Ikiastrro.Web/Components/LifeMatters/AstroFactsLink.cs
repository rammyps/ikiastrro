namespace Ikiastrro.Web.Components.LifeMatters;

/// <summary>
/// Links from Key Inference back to the Astro Facts tables behind a reading:
/// <c>/astro-facts/{id}?step=houses&amp;chart=D9</c>. <c>step</c> is one of Astro Facts's own
/// step names (AstroFacts.razor OnParametersSet); <c>chart</c> opens its chart picker on that
/// chart and is left out for D1, the picker's default.
/// </summary>
public static class AstroFactsLink
{
    public const string Overview = "overview";
    public const string Planets = "planets";
    public const string Houses = "houses";
    public const string Relationships = "relationships";
    public const string Strength = "strength";
    public const string SpecialLagnas = "spllagnas";

    public static string Url(int personId, string step, string? chart = null) =>
        $"/astro-facts/{personId}?step={Uri.EscapeDataString(step)}"
        + (string.IsNullOrEmpty(chart) || chart == "D1" ? "" : $"&chart={Uri.EscapeDataString(chart)}");
}
