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

        // Other Tamil Nadu cities/towns (Nominatim, 2026-09-28). Common alternate spellings get
        // their own row with the same coordinates, since matching is on the exact city name.
        // OSM relation 1766358 — Chennai
        new("Chennai", "India", 13.0836939, 80.2701860, "Asia/Kolkata"),
        new("Madras", "India", 13.0836939, 80.2701860, "Asia/Kolkata"),
        // OSM relation 11268397 — Madurai
        new("Madurai", "India", 9.9261153, 78.1140983, "Asia/Kolkata"),
        // OSM relation 10318360 — Tiruchirappalli
        new("Tiruchirappalli", "India", 10.8071144, 78.6880939, "Asia/Kolkata"),
        new("Trichy", "India", 10.8071144, 78.6880939, "Asia/Kolkata"),
        // OSM node 367564056 — Srirangam, Tiruchirappalli, 620006
        new("Srirangam", "India", 10.8573308, 78.6930848, "Asia/Kolkata"),
        // OSM node 245587589 — Thanjavur, 613001
        new("Thanjavur", "India", 10.7860267, 79.1381497, "Asia/Kolkata"),
        new("Tanjore", "India", 10.7860267, 79.1381497, "Asia/Kolkata"),
        // OSM node 245588875 — Kumbakonam, 612001
        new("Kumbakonam", "India", 10.9604108, 79.3820861, "Asia/Kolkata"),
        // OSM node 2271360789 — Mayiladuthurai, 609129
        new("Mayiladuthurai", "India", 11.1018614, 79.6503162, "Asia/Kolkata"),
        // OSM node 891394162 — Nagapattinam, 611001
        new("Nagapattinam", "India", 10.7647952, 79.8430779, "Asia/Kolkata"),
        // OSM way 84749701 — Pudukkottai (town)
        new("Pudukkottai", "India", 10.3826515, 78.8191259, "Asia/Kolkata"),
        // OSM way 82623598 — Karur (city)
        new("Karur", "India", 10.9596041, 78.0807797, "Asia/Kolkata"),
        // OSM node 245586375 — Dindigul, 624001
        new("Dindigul", "India", 10.3656460, 77.9693256, "Asia/Kolkata"),
        // OSM node 3780703409 — Salem, 636001
        new("Salem", "India", 11.6551982, 78.1581771, "Asia/Kolkata"),
        // OSM node 314626472 — Erode, 638001
        new("Erode", "India", 11.3306483, 77.7276519, "Asia/Kolkata"),
        // OSM node 4437851490 — Tiruppur, 638600
        new("Tiruppur", "India", 11.1017815, 77.3451920, "Asia/Kolkata"),
        // OSM node 983364958 — Hosur, 635109
        new("Hosur", "India", 12.7328844, 77.8309478, "Asia/Kolkata"),
        // OSM node 243713144 — Vellore, 632012
        new("Vellore", "India", 12.9071753, 79.1309695, "Asia/Kolkata"),
        // OSM node 256777854 — Kanchipuram, 631501
        new("Kanchipuram", "India", 12.8363930, 79.7053304, "Asia/Kolkata"),
        // OSM way 84649892 — Tiruvannamalai (town)
        new("Tiruvannamalai", "India", 12.2343228, 79.0761989, "Asia/Kolkata"),
        // OSM node 4592194577 — Cuddalore, 607001
        new("Cuddalore", "India", 11.7564329, 79.7634644, "Asia/Kolkata"),
        // OSM node 891394174 — Chidambaram, 608001
        new("Chidambaram", "India", 11.3994826, 79.6909383, "Asia/Kolkata"),
        // OSM node 245581857 — Tirunelveli, 627001
        new("Tirunelveli", "India", 8.7331721, 77.7102550, "Asia/Kolkata"),
        // OSM node 5443775116 — Thoothukudi, 628001
        new("Thoothukudi", "India", 8.8052602, 78.1452745, "Asia/Kolkata"),
        new("Tuticorin", "India", 8.8052602, 78.1452745, "Asia/Kolkata"),
        // OSM node 245580128 — Nagercoil, 629001
        new("Nagercoil", "India", 8.1839904, 77.4315437, "Asia/Kolkata"),
        // OSM node 339884241 — Rameswaram, 623526
        new("Rameswaram", "India", 9.2844657, 79.3125553, "Asia/Kolkata"),
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
