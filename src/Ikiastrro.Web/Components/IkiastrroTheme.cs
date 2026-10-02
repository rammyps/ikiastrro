using MudBlazor;

namespace Ikiastrro.Web.Components;

/// <summary>
/// App-wide MudBlazor theme wired to the adaptive Cosmos palette.
///
/// Hex values are duplicated from <c>wwwroot/css/tokens.css</c> because a
/// <see cref="MudTheme"/> is C# and cannot read CSS custom properties. Keep the
/// two in sync; the CSS tokens remain the source of truth for everything else.
/// </summary>
public static class IkiastrroTheme
{
    private const string LightCanvas = "#F8FAFC";
    private const string LightSurface = "#FFFFFF";
    private const string LightInk = "#172033";
    private const string LightMuted = "#526176";
    private const string LightLine = "#CBD5E1";
    private const string LightAccent = "#087E9A";

    private const string DarkCanvas = "#0B0F19";
    private const string DarkSurface = "#111827";
    private const string DarkInk = "#F8FAFC";
    private const string DarkMuted = "#94A3B8";
    private const string DarkLine = "#334155";
    private const string DarkAccent = "#22D3EE";

    public static readonly MudTheme DefaultLight = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#FCD7BD",
            PrimaryContrastText = "#0F2041",
            Secondary = "#0F2041",
            Background = "#FAF5EA",
            BackgroundGray = "#FAF5EA",
            Surface = "#FAF5EA",
            AppbarBackground = "#FCD7BD",
            AppbarText = "#0F2041",
            DrawerBackground = "#FAF5EA",
            DrawerText = "#0F2041",
            TextPrimary = "#0F2041",
            TextSecondary = "#53698D",
            ActionDefault = "#0F2041",
            Divider = "#D9D1C7",
            LinesDefault = "#D9D1C7",
            LinesInputs = "#D9D1C7",
            TableLines = "#D9D1C7",
        },
    };

    public static readonly MudTheme Cosmos = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = LightAccent,
            PrimaryContrastText = "#FFFFFF",
            Secondary = LightInk,
            Background = LightCanvas,
            BackgroundGray = "#F1F5F9",
            Surface = LightSurface,
            AppbarBackground = LightSurface,
            AppbarText = LightInk,
            DrawerBackground = LightSurface,
            DrawerText = LightInk,
            TextPrimary = LightInk,
            TextSecondary = LightMuted,
            ActionDefault = LightInk,
            Divider = LightLine,
            LinesDefault = LightLine,
            LinesInputs = LightLine,
            TableLines = LightLine,
        },
        PaletteDark = new PaletteDark
        {
            Primary = DarkAccent,
            PrimaryContrastText = DarkCanvas,
            Secondary = DarkMuted,
            Background = DarkCanvas,
            BackgroundGray = "#172033",
            Surface = DarkSurface,
            AppbarBackground = DarkSurface,
            AppbarText = DarkInk,
            DrawerBackground = DarkSurface,
            DrawerText = DarkInk,
            TextPrimary = DarkInk,
            TextSecondary = DarkMuted,
            ActionDefault = DarkInk,
            Divider = DarkLine,
            LinesDefault = DarkLine,
            LinesInputs = DarkLine,
            TableLines = DarkLine,
        },
    };

    public static readonly MudTheme NebulaLight = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#FF9F3D",
            PrimaryContrastText = "#4B0082",
            Secondary = "#8A2BE2",
            SecondaryContrastText = "#FFFFFF",
            Background = "#FFF9EC",
            BackgroundGray = "#FFF0CF",
            Surface = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#4B0082",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#4B0082",
            TextPrimary = "#4B0082",
            TextSecondary = "#70557E",
            ActionDefault = "#4B0082",
            Divider = "#E4CCE9",
            LinesDefault = "#E4CCE9",
            LinesInputs = "#E4CCE9",
            TableLines = "#E4CCE9",
        },
    };
}
