namespace Ikiastrro.Web.Components.Charts;

/// <summary>One planet's line inside a D1TemplateGrid cell — glyph, whole degrees within the sign,
/// and its retrograde/combust flags (rendered as "(..)" parens and a 🔥, matching SouthIndianGrid's
/// existing direction-suffix/combust-icon convention).</summary>
public sealed record TemplatePlanetLine(string Glyph, int DegreesInSign, bool IsRetrograde, bool IsCombust);

/// <summary>One karaka/special-point line for D1TemplateGrid's Ras(L)/Hor(L) header row — a short
/// code (a CharaKaraka like "Ak", or a special-point code like "HL"/"AL"/"A7") flanked by its house
/// count from Lagna and from the natal Moon — the same two-axis gold/silver idea SouthIndianGrid
/// already uses for planet house badges, generalized here to karakas and special points.</summary>
public sealed record TemplatePointLine(string Code, int HouseFromLagna, int HouseFromMoon);
