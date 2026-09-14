using System.Globalization;
using System.Text;
using Ikiastrro.Core.Geocoding;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Data;

/// <summary>One row's outcome from <see cref="BirthDetailsCsvService.ImportAsync"/>.</summary>
public record CsvImportResult(int Added, int Skipped, IReadOnlyList<string> Errors);

/// <summary>
/// In-process CSV import/export for <c>tbl_BirthDetails</c> (docs/ui/components/saved-people.md,
/// decisions/002). Same 10-column header as <c>tools/jkd-interchange/jkd-csv.ps1</c>'s CSV shape
/// (Name, Sex, DateOfBirth yyyy-MM-dd, TimeOfBirth HH:mm:ss, City, Country, Latitude, Longitude,
/// UtcOffset, IanaTimeZoneId), so a file round-trips between the two paths.
///
/// Import never geocodes a row that already carries Latitude/Longitude — <see cref="IPlaceResolver"/>
/// only fires for a row whose coordinates are blank (per spec: "Import does not geocode"). Dedupe is
/// by Name against the existing <c>UX_BirthDetails_Name</c> rows; a bad row is skipped with an error
/// message rather than aborting the whole file, mirroring how a delete that throws is caught and
/// shown inline elsewhere on this page rather than crashing the circuit.
/// </summary>
public class BirthDetailsCsvService
{
    private static readonly string[] Headers =
        { "Name", "Sex", "DateOfBirth", "TimeOfBirth", "City", "Country", "Latitude", "Longitude", "UtcOffset", "IanaTimeZoneId" };

    private readonly BirthDetailsRepository _repo;
    private readonly IPlaceResolver _placeResolver;

    public BirthDetailsCsvService(BirthDetailsRepository repo, IPlaceResolver placeResolver)
    {
        _repo = repo;
        _placeResolver = placeResolver;
    }

    public async Task<CsvImportResult> ImportAsync(Stream csv)
    {
        var rows = await ParseCsvAsync(csv);
        if (rows.Count == 0) return new CsvImportResult(0, 0, Array.Empty<string>());

        var header = rows[0];
        var missing = Headers.Where(h => !header.Contains(h)).ToList();
        if (missing.Count > 0)
            return new CsvImportResult(0, 0, new[] { $"Missing CSV header(s): {string.Join(", ", missing)}." });

        var colIndex = header.Select((h, i) => (h, i)).ToDictionary(x => x.h, x => x.i);
        string Field(IReadOnlyList<string> r, string name) =>
            colIndex.TryGetValue(name, out var i) && i < r.Count ? r[i] : "";

        var added = 0;
        var skipped = 0;
        var errors = new List<string>();

        foreach (var r in rows.Skip(1))
        {
            var name = Field(r, "Name").Trim();
            if (name.Length == 0) continue; // blank/trailing row

            try
            {
                if (_repo.ExistsByName(name)) { skipped++; continue; }

                var dob = DateOnly.ParseExact(Field(r, "DateOfBirth").Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var tob = TimeOnly.ParseExact(Field(r, "TimeOfBirth").Trim(), "HH:mm:ss", CultureInfo.InvariantCulture);
                var city = Field(r, "City").Trim();
                var country = Field(r, "Country").Trim();
                var sex = Field(r, "Sex").Trim();
                var latText = Field(r, "Latitude").Trim();
                var lonText = Field(r, "Longitude").Trim();
                var iana = Field(r, "IanaTimeZoneId").Trim();

                double lat, lon;
                string offset;
                string? ianaId = iana.Length == 0 ? null : iana;

                if (latText.Length == 0 || lonText.Length == 0)
                {
                    // Only path that geocodes — the row didn't carry its own coordinates.
                    var place = await _placeResolver.ResolveAsync(city, country, dob);
                    lat = place.Latitude;
                    lon = place.Longitude;
                    offset = place.UtcOffset.ToString();   // same expression SavedCharts.razor's edit-save uses
                    ianaId = place.IanaTimeZoneId;
                }
                else
                {
                    lat = double.Parse(latText, CultureInfo.InvariantCulture);
                    lon = double.Parse(lonText, CultureInfo.InvariantCulture);
                    offset = Field(r, "UtcOffset").Trim();
                    if (offset.Length == 0)
                        throw new FormatException("UtcOffset is required when Latitude/Longitude are given.");
                }

                _repo.Insert(new BirthDetails
                {
                    Name = name,
                    Sex = sex.Length == 0 ? null : sex,
                    DateOfBirth = dob,
                    TimeOfBirth = tob,
                    PlaceCity = city,
                    PlaceCountry = country,
                    Latitude = lat,
                    Longitude = lon,
                    UtcOffset = offset,
                    IanaTimeZoneId = ianaId,
                    CreatedAt = DateTime.UtcNow,
                });
                added++;
            }
            catch (Exception ex)
            {
                errors.Add($"{name}: {ex.Message}");
            }
        }

        return new CsvImportResult(added, skipped, errors);
    }

    public string ExportCsv(IEnumerable<BirthDetails> people)
    {
        var sb = new StringBuilder();
        sb.Append(string.Join(",", Headers)).Append("\r\n");
        foreach (var p in people)
        {
            sb.Append(string.Join(",", new[]
            {
                Escape(p.Name), Escape(p.Sex ?? ""), p.DateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                p.TimeOfBirth.ToString("HH:mm:ss", CultureInfo.InvariantCulture), Escape(p.PlaceCity), Escape(p.PlaceCountry),
                p.Latitude.ToString("0.######", CultureInfo.InvariantCulture),
                p.Longitude.ToString("0.######", CultureInfo.InvariantCulture),
                Escape(p.UtcOffset), Escape(p.IanaTimeZoneId ?? ""),
            })).Append("\r\n");
        }
        return sb.ToString();
    }

    // --- minimal RFC 4180 CSV, no external dependency (the app has no CSV library, and this is
    // the only 10-column shape it ever needs to read/write) ---

    private static string Escape(string field) =>
        field.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 ? $"\"{field.Replace("\"", "\"\"")}\"" : field;

    private static async Task<List<List<string>>> ParseCsvAsync(Stream stream)
    {
        // ReadToEndAsync, not ReadToEnd — Blazor's IBrowserFile stream (InputFile.OpenReadStream)
        // rejects synchronous reads outright ("Synchronous reads are not supported").
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var text = await reader.ReadToEndAsync();

        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else inQuotes = false;
                }
                else field.Append(c);
            }
            else if (c == '"') inQuotes = true;
            else if (c == ',') { row.Add(field.ToString()); field.Clear(); }
            else if (c == '\r') { /* swallow; \n (or end of text) closes the row */ }
            else if (c == '\n') { row.Add(field.ToString()); field.Clear(); rows.Add(row); row = new List<string>(); }
            else field.Append(c);
        }
        if (field.Length > 0 || row.Count > 0) { row.Add(field.ToString()); rows.Add(row); }

        // Drop wholly-blank trailing lines (e.g. the file's final newline).
        return rows.Where(r => r.Any(f => f.Trim().Length > 0)).ToList();
    }
}
