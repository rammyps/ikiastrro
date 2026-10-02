using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;

namespace Ikiastrro.Core.Engines.KeyInfo;

/// <summary>The Yogi point and its planets for one chart.</summary>
public sealed record YogiResult(double YogiPoint, PlanetName Yogi, PlanetName Sahayogi, double AvayogiPoint, PlanetName Avayogi);

/// <summary>
/// Yogi, Sahayogi and Avayogi (SRC_PVR_INTEGRATED). Yogi point = Sun + Moon + 93°20′; the Yogi is the
/// lord of its nakṣatra and the Sahayogi (duplicate Yogi) the lord of its sign. Avayogi point = Yogi
/// point + 186°40′; the Avayogi is the lord of its nakṣatra. JHora 1_Ramakrishnan: Yogi Rahu at
/// 18°44′ Aquarius, Sahayogi Saturn, Avayogi Venus at 25°24′ Leo.
/// </summary>
public static class YogiAvayogi
{
    public static YogiResult Compute(double sunLongitude, double moonLongitude)
    {
        var yogi = AstroMath.Normalize(sunLongitude + moonLongitude + 93 + 1 / 3.0);
        var avayogi = AstroMath.Normalize(yogi + 186 + 2 / 3.0);
        return new YogiResult(yogi, AstroMath.GetNakshatraLord(yogi),
            Enum.Parse<PlanetName>(HouseEngine.GetSignLord((ZodiacName)(int)(yogi / 30))), avayogi, AstroMath.GetNakshatraLord(avayogi));
    }
}
