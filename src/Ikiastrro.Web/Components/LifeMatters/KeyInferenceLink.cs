namespace Ikiastrro.Web.Components.LifeMatters;

/// <summary>
/// Links from Life Matters back to the Key Inference tables behind a reading:
/// <c>/key-inference/{id}?step=houses&amp;chart=D9</c>. <c>step</c> is one of Key Inference's own
/// step names (KeyInference.razor OnParametersSet); <c>chart</c> opens its chart picker on that
/// chart and is left out for D1, the picker's default.
/// </summary>
public static class KeyInferenceLink
{
    public const string Overview = "overview";
    public const string Planets = "planets";
    public const string Houses = "houses";
    public const string Relationships = "relationships";
    public const string Strength = "strength";
    public const string SpecialLagnas = "spllagnas";

    public static string Url(int personId, string step, string? chart = null) =>
        $"/key-inference/{personId}?step={Uri.EscapeDataString(step)}"
        + (string.IsNullOrEmpty(chart) || chart == "D1" ? "" : $"&chart={Uri.EscapeDataString(chart)}");
}
