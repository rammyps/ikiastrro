using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Web.Components.Charts;

public sealed record SindHovHouseFocus(string ReferenceSign, int HouseNumber);

public static class LifeMatterSignMath
{
    public static string ResolveHouseSign(string referenceSign, int houseNumber)
    {
        if (!Enum.TryParse<ZodiacName>(referenceSign, out var origin))
            throw new ArgumentException($"Unknown reference sign '{referenceSign}'.", nameof(referenceSign));

        if (houseNumber is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(houseNumber), houseNumber, "House number must be from 1 through 12.");

        return Enum.GetValues<ZodiacName>()
            .Single(sign => AstroMath.CountFromSignToSign(origin, sign) == houseNumber)
            .ToString();
    }
}
