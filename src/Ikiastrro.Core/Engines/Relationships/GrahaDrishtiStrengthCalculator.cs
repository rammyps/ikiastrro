using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Relationships;

public sealed record GrahaDrishtiStrengthResult(
    double DirectedSeparationDegrees,
    double OrdinaryVirupas,
    double SpecialVirupas,
    double TotalVirupas,
    double Percentage,
    int? DiscreteAspectHouse)
{
    public bool IsDiscreteAspect => DiscreteAspectHouse.HasValue;
}

/// <summary>
/// Calculates Parashari sphuta graha-drishti strength. The longitude-based strength and
/// whole-sign discrete aspect are deliberately returned as separate facts: a non-zero
/// strength does not imply a recognized whole-sign aspect.
/// </summary>
public static class GrahaDrishtiStrengthCalculator
{
    public static GrahaDrishtiStrengthResult Calculate(
        PlanetName aspectingPlanet,
        double aspectingLongitudeDegrees,
        double aspectedLongitudeDegrees)
    {
        ValidateLongitude(aspectingLongitudeDegrees, nameof(aspectingLongitudeDegrees));
        ValidateLongitude(aspectedLongitudeDegrees, nameof(aspectedLongitudeDegrees));

        var separation = Normalize(aspectedLongitudeDegrees - aspectingLongitudeDegrees);
        var ordinary = OrdinaryContribution(separation);
        var special = SpecialContribution(aspectingPlanet, separation);
        var total = Math.Min(ordinary + special, 60.0);
        var percentage = Math.Round(total / 60.0 * 100.0, 2, MidpointRounding.AwayFromZero);
        var aspectHouse = DiscreteAspectHouse(
            aspectingPlanet,
            (int)Math.Floor(aspectingLongitudeDegrees / 30.0),
            (int)Math.Floor(aspectedLongitudeDegrees / 30.0));

        return new GrahaDrishtiStrengthResult(
            separation, ordinary, special, total, percentage, aspectHouse);
    }

    private static double OrdinaryContribution(double angle) => angle switch
    {
        < 30.0 => 0.0,
        < 60.0 => 0.5 * (angle - 30.0),
        < 90.0 => angle - 45.0,
        < 120.0 => 0.5 * (120.0 - angle) + 30.0,
        < 150.0 => 150.0 - angle,
        < 180.0 => 2.0 * (angle - 150.0),
        < 300.0 => 0.5 * (300.0 - angle),
        _ => 0.0
    };

    private static double SpecialContribution(PlanetName planet, double angle) => planet switch
    {
        PlanetName.Mars when angle is >= 60.0 and < 90.0 => 0.5 * (angle - 60.0),
        PlanetName.Mars when angle is >= 90.0 and < 120.0 => 15.0,
        PlanetName.Mars when angle is >= 210.0 and < 240.0 => 15.0,

        PlanetName.Jupiter when angle is >= 90.0 and < 120.0 => angle - 90.0,
        PlanetName.Jupiter when angle is >= 120.0 and < 150.0 => 30.0,
        PlanetName.Jupiter when angle is >= 240.0 and < 270.0 => 30.0,

        PlanetName.Saturn when angle is >= 30.0 and < 60.0 => 1.5 * (angle - 30.0),
        PlanetName.Saturn when angle is >= 60.0 and < 90.0 => 45.0,
        PlanetName.Saturn when angle is >= 270.0 and < 300.0 => 45.0,
        _ => 0.0
    };

    private static int? DiscreteAspectHouse(PlanetName planet, int sourceSign, int targetSign)
    {
        var house = (targetSign - sourceSign + 12) % 12 + 1;
        return house == 7
            || planet == PlanetName.Mars && house is 4 or 8
            || planet == PlanetName.Jupiter && house is 5 or 9
            || planet == PlanetName.Saturn && house is 3 or 10
                ? house
                : null;
    }

    private static double Normalize(double degrees) => (degrees % 360.0 + 360.0) % 360.0;

    private static void ValidateLongitude(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value is < 0.0 or >= 360.0)
            throw new ArgumentOutOfRangeException(parameterName, value, "Longitude must be finite and in [0, 360).");
    }
}
