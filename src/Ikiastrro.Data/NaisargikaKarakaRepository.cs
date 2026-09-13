using Dapper;

namespace Ikiastrro.Data;

public sealed record NaisargikaKarakaRow(int HouseNumber, string Graha, string MattersSignified);
public sealed record NaisargikaKarakatwaRow(int HouseNumber, string Graha, string Matter, int DisplayOrder);
public sealed record NaisargikaKarakaRules(IReadOnlyList<NaisargikaKarakaRow> Primary, IReadOnlyList<NaisargikaKarakatwaRow> Details);

public sealed class NaisargikaKarakaRepository(SqlConnectionFactory factory)
{
    public NaisargikaKarakaRules LoadActive()
    {
        using var connection = factory.CreateOpenConnection();
        var primary = connection.Query<NaisargikaKarakaRow>("""
            SELECT nk.HouseNumber, p.PlanetName AS Graha, nk.MattersSignified
            FROM dbo.tbl_Rule_Naisargika_Karakas nk
            JOIN dbo.tbl_Rule_Sets rs ON rs.Id = nk.RuleSetId AND rs.IsActive = 1
            JOIN dbo.tbl_Planets p ON p.Id = nk.GrahaId
            WHERE nk.IsActive = 1 ORDER BY nk.HouseNumber
            """).ToList();
        var details = connection.Query<NaisargikaKarakatwaRow>("""
            SELECT nk.HouseNumber, p.PlanetName AS Graha, nk.Matter, nk.DisplayOrder
            FROM dbo.tbl_Rule_Naisargika_Karakatwas nk
            JOIN dbo.tbl_Rule_Sets rs ON rs.Id = nk.RuleSetId AND rs.IsActive = 1
            JOIN dbo.tbl_Planets p ON p.Id = nk.GrahaId
            WHERE nk.IsActive = 1 ORDER BY nk.HouseNumber, nk.DisplayOrder, p.Id
            """).ToList();
        return new(primary, details);
    }
}
