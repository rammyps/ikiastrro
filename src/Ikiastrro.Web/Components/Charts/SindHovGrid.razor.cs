using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.LifeMatters;

namespace Ikiastrro.Web.Components.Charts;

public sealed record SindHovHouseFocus(string ReferenceSign, int HouseNumber);

/// <summary>
/// String-keyed wrapper over <see cref="LifeMatterFocusResolver.ResolveHouseSign"/> — the grid
/// works in sign names (its cells, parameters, and data-* attributes are all strings), while the
/// resolver works in <see cref="ZodiacName"/>. Kept as one formula, one call site.
/// </summary>
public static class LifeMatterSignMath
{
    public static string ResolveHouseSign(string referenceSign, int houseNumber)
    {
        if (!Enum.TryParse<ZodiacName>(referenceSign, out var origin))
            throw new ArgumentException($"Unknown reference sign '{referenceSign}'.", nameof(referenceSign));

        return LifeMatterFocusResolver.ResolveHouseSign(origin, houseNumber).ToString();
    }
}
