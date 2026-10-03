using Dapper;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Matching;

namespace Ikiastrro.Data;

/// <summary>A saved person's Kuta-matching inputs plus the labels a page shows next to them.</summary>
public sealed record MatchPersonSnapshot(
    int BirthDetailId, string Name, string? Sex, MatchPerson Person, string NakshatraName, int Pada);

/// <summary>
/// Reads the Moon facts Kuta matching needs from what is already stored: the D1 Moon row in
/// tbl_Chart_KeyDetails (sign, nakshatra, pada) joined to tbl_Nakshatras (Gana, Yoni, Nadi —
/// migration 098). Nothing is written and nothing is recomputed, so a person's inputs always agree
/// with the chart on screen.
/// </summary>
public sealed class MatchPersonRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public MatchPersonRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    private sealed record Row(
        int BirthDetailId, string Name, string? Sex, string Sign, byte NakshatraId, string NakshatraName, byte? Pada,
        string? Gana, string? YoniAnimal, string? YoniGender, string? Nadi);

    /// <summary>Null when the person has no stored D1 Moon, or the nakshatra reference rows are not
    /// seeded (Gana, Yoni or Nadi missing), so matching cannot be read.</summary>
    public MatchPersonSnapshot? GetByBirthDetailId(int birthDetailId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        var row = connection.QuerySingleOrDefault<Row>("""
            SELECT b.Id AS BirthDetailId, b.Name, b.Sex, k.Sign, n.Id AS NakshatraId, n.NakshatraName,
                   k.NakshatraPada AS Pada, n.Gana, n.YoniAnimal, n.YoniGender, n.Nadi
            FROM dbo.tbl_BirthDetails b
            JOIN dbo.tbl_ChartResults c ON c.BirthDetailId = b.Id AND c.ChartType = 'D1'
            JOIN dbo.tbl_Chart_KeyDetails k ON k.ChartResultId = c.Id AND k.Planet = 'Moon' AND k.PointKind = 'Graha'
            JOIN dbo.tbl_Nakshatras n ON n.Id = k.NakshatraId
            WHERE b.Id = @Id
            """, new { Id = birthDetailId });
        if (row is null || row.Gana is null || row.YoniAnimal is null || row.YoniGender is null || row.Nadi is null)
            return null;
        if (!Enum.TryParse<ZodiacName>(row.Sign, out var sign)) return null;

        return new MatchPersonSnapshot(
            row.BirthDetailId, row.Name, row.Sex,
            new MatchPerson(sign, row.NakshatraId, row.Gana, row.YoniAnimal, row.YoniGender, row.Nadi),
            row.NakshatraName, row.Pada ?? 0);
    }
}
