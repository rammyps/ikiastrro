namespace Ikiastrro.Web.Components.Charts;

/// <summary>
/// One planet glyph to render inside a <see cref="SouthIndianGrid_Micro"/> cell — sibling of
/// <see cref="GridPlanetGlyph"/> (kept as a separate record, not an edit to that one, per
/// project_standards.md §3.3: a new module gets its own types, an existing consumer's shape
/// never changes out from under it). Adds the two karaka tags Detailed has no concept of.
///
/// <paramref name="NaisargikaKarakaTag"/>/<paramref name="CharaKarakaTag"/> are pre-formatted
/// display strings (e.g. "NK1", "AK") or null — the caller (<see cref="PolarGridLagnaSelect"/>)
/// decides whether to populate them at all (both stay null outside D1/D9, a display-only gate;
/// see spec_SouthIndianGrid_Micro.md).
/// </summary>
public sealed record MicroPlanetGlyph(
    string PlanetName, string? DignityToken, bool IsRetrograde, bool IsCombust,
    string? NaisargikaKarakaTag, string? CharaKarakaTag);
