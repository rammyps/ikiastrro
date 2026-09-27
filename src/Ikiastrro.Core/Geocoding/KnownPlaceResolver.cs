using TimeZoneConverter;

namespace Ikiastrro.Core.Geocoding;

/// <summary>
/// Built-in list of frequently used birth places, checked before falling through to the wrapped
/// resolver (normally <see cref="NominatimPlaceResolver"/>). A listed place resolves offline and
/// always to the same coordinates, so repeat charts for the same city never drift with changes
/// in the online geocoder's data.
///
/// UTC offset is still derived from the IANA zone + birth date, same as the Nominatim path.
/// To add a place, append a row to <see cref="Places"/> (coordinates from OpenStreetMap Nominatim).
/// </summary>
public class KnownPlaceResolver : IPlaceResolver
{
    private record KnownPlace(string City, string Country, double Latitude, double Longitude, string IanaTimeZoneId);

    private static readonly KnownPlace[] Places =
    [
        // OSM node 245589078 — Coimbatore, Tamil Nadu, 641001
        new("Coimbatore", "India", 11.0018115, 76.9628425, "Asia/Kolkata"),
    ];

    private readonly IPlaceResolver _fallback;

    public KnownPlaceResolver(IPlaceResolver fallback)
    {
        _fallback = fallback;
    }

    public Task<ResolvedPlace> ResolveAsync(string city, string country, DateOnly onDate)
    {
        var match = Places.FirstOrDefault(p =>
            string.Equals(p.City, city.Trim(), StringComparison.OrdinalIgnoreCase) &&
            string.Equals(p.Country, country.Trim(), StringComparison.OrdinalIgnoreCase));

        if (match is null)
            return _fallback.ResolveAsync(city, country, onDate);

        var timeZoneInfo = TZConvert.GetTimeZoneInfo(match.IanaTimeZoneId);
        var utcOffset = timeZoneInfo.GetUtcOffset(onDate.ToDateTime(new TimeOnly(12, 0)));

        return Task.FromResult(new ResolvedPlace(match.Latitude, match.Longitude, match.IanaTimeZoneId, utcOffset));
    }
}
