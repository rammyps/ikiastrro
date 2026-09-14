using Dapper;

namespace Ikiastrro.Data;

public sealed record NaisargikaKarakaRow(int HouseNumber, string Graha, string MattersSignified);
public sealed record NaisargikaKarakatwaRow(int HouseNumber, string Graha, string Matter, int DisplayOrder);
public sealed record LifeMatterReferenceRow(
    string CategoryName, int DisplayOrder, string MatterText, string PrimaryChartsText,
    string HouseFromLagnaText, string KarakaText, string? CharaKarakaCode,
    string HouseFromKarakaText, string BasisCode);
public sealed record HouseReadingRow(int HouseNumber, string HouseName, string HouseSummary);
public sealed record SignReadingRow(string SignName, string SignIndication);
public sealed record SignNakshatraRelationshipRow(
    string RasiName, string RasiLordName, string NakshatraName, string NakshatraLordName,
    string? RasiLordNakshatraLordRelationship, string SubLordName, string? RasiLordSubLordRelationship);
public sealed record LagnaReferenceRow(string ReferenceCode, string ReferenceName, string? Abbreviation, string Perspective, string AppliesInVarga, int SortOrder);
public sealed record NaisargikaKarakaRules(
    IReadOnlyList<NaisargikaKarakaRow> Primary,
    IReadOnlyList<NaisargikaKarakatwaRow> Details,
    IReadOnlyList<LifeMatterReferenceRow> LifeMatters,
    IReadOnlyList<HouseReadingRow> Houses,
    IReadOnlyList<SignReadingRow> Signs,
    IReadOnlyList<SignNakshatraRelationshipRow> SignNakshatraRelationships,
    IReadOnlyList<LagnaReferenceRow> LagnaReferences);

public sealed class NaisargikaKarakaRepository(SqlConnectionFactory factory)
{
    public NaisargikaKarakaRules LoadActive()
    {
        using var connection = factory.CreateOpenConnection();
        var primary = connection.Query<NaisargikaKarakaRow>("""
            SELECT CAST(nk.HouseNumber AS INT) AS HouseNumber, p.PlanetName AS Graha, nk.MattersSignified
            FROM dbo.tbl_Rule_Naisargika_Karakas nk
            JOIN dbo.tbl_Rule_Sets rs ON rs.Id = nk.RuleSetId AND rs.IsActive = 1
            JOIN dbo.tbl_Planets p ON p.Id = nk.GrahaId
            WHERE nk.IsActive = 1 ORDER BY nk.HouseNumber
            """).ToList();
        var details = connection.Query<NaisargikaKarakatwaRow>("""
            SELECT CAST(nk.HouseNumber AS INT) AS HouseNumber, p.PlanetName AS Graha, nk.Matter, CAST(nk.DisplayOrder AS INT) AS DisplayOrder
            FROM dbo.tbl_Rule_Naisargika_Karakatwas nk
            JOIN dbo.tbl_Rule_Sets rs ON rs.Id = nk.RuleSetId AND rs.IsActive = 1
            JOIN dbo.tbl_Planets p ON p.Id = nk.GrahaId
            WHERE nk.IsActive = 1 ORDER BY nk.HouseNumber, nk.DisplayOrder, p.Id
            """).ToList();
        var lifeMatters = connection.Query<LifeMatterReferenceRow>("""
            SELECT lm.CategoryName, CAST(lm.DisplayOrder AS INT) AS DisplayOrder,
                   lm.MatterText, lm.PrimaryChartsText, lm.HouseFromLagnaText,
                   lm.KarakaText, lm.CharaKarakaCode, lm.HouseFromKarakaText, lm.BasisCode
            FROM dbo.tbl_Rule_LifeMatterReference lm
            JOIN dbo.tbl_Rule_Sets rs ON rs.Id = lm.RuleSetId AND rs.IsActive = 1
            WHERE lm.IsActive = 1
            ORDER BY lm.CategoryName, lm.DisplayOrder, lm.Id
            """).ToList();
        var houses = connection.Query<HouseReadingRow>("""
            SELECT h.HouseNumber, COALESCE(h.EnglishName, h.ShortName) AS HouseName,
                   STRING_AGG(s.SignificationText, ', ') WITHIN GROUP (ORDER BY s.DisplayOrder) AS HouseSummary
            FROM dbo.tbl_Dim_House h
            JOIN dbo.tbl_Rule_HouseSignification s ON s.HouseNumber = h.HouseNumber AND s.IsActive = 1
            WHERE h.IsActive = 1 AND s.SignificationCategory = 'Matter'
            GROUP BY h.HouseNumber, h.EnglishName, h.ShortName ORDER BY h.HouseNumber
            """).ToList();
        var signs = connection.Query<SignReadingRow>("""
            SELECT SignName, SignIndication FROM dbo.tbl_SignAttributes ORDER BY Id
            """).ToList();
        var relationships = connection.Query<SignNakshatraRelationshipRow>("""
            SELECT RasiName, RasiLordName, NakshatraName, NakshatraLordName,
                   RasiLord_NakshatraLord_Relationship AS RasiLordNakshatraLordRelationship,
                   SubLordName, RasiLord_SubLord_Relationship AS RasiLordSubLordRelationship
            FROM dbo.vw_Rule_SignNakshatraRelationship
            """).ToList();
        var references = connection.Query<LagnaReferenceRow>("""
            SELECT r.ReferenceCode, r.ReferenceName, l.Abbreviation, r.Perspective, r.AppliesInVarga, CAST(r.SortOrder AS INT) AS SortOrder
            FROM dbo.tbl_Dim_HouseReference r
            LEFT JOIN dbo.tbl_Dim_SpecialLagnas l ON l.Id = r.BasisSpecialLagnaId
            WHERE r.IsActive = 1 AND (r.ReferenceCode = 'LAGNA' OR r.BasisKind = 'SpecialLagna')
            ORDER BY r.SortOrder
            """).ToList();
        return new(primary, details, lifeMatters, houses, signs, relationships, references);
    }
}
