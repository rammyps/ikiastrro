using Dapper;

namespace Ikiastrro.Data;

public sealed record InterpretiveFactorRow(int Id, string FactorCode, string FactorName, string Description, int SortOrder);

public sealed record InterpretiveFactorDetailRow(
    string FactorCode, int? HouseNumber, string? GrahaName, string? SignName, string? ChartTypeCode,
    bool IsPrimary, int DisplayOrder, string? Notes);

/// <summary>
/// dbo.tbl_Dim_InterpretiveFactor + dbo.tbl_Rule_InterpretiveFactorDetail (migration 109) —
/// House/Planet/Sign/Varga facts normalized out of tbl_Dim_LifeArea, tbl_Dim_DivisionalSubject
/// and CHARA-typed tbl_Dim_KarakaRole rows. Read-only.
/// </summary>
public sealed class InterpretiveFactorDetailRepository(SqlConnectionFactory factory)
{
    private const string DetailSelect = """
        SELECT f.FactorCode, CAST(d.HouseNumber AS INT) AS HouseNumber, p.PlanetName AS GrahaName,
               sa.SignName, ct.Code AS ChartTypeCode, d.IsPrimary, CAST(d.DisplayOrder AS INT) AS DisplayOrder, d.Notes
        FROM dbo.tbl_Rule_InterpretiveFactorDetail d
        JOIN dbo.tbl_Rule_Sets rs ON rs.Id = d.RuleSetId AND rs.IsActive = 1
        JOIN dbo.tbl_Dim_InterpretiveFactor f ON f.Id = d.FactorId
        LEFT JOIN dbo.tbl_Planets p ON p.Id = d.GrahaId
        LEFT JOIN dbo.tbl_SignAttributes sa ON sa.Id = d.SignId
        LEFT JOIN dbo.tbl_Dim_ChartType ct ON ct.Id = d.ChartTypeId
        WHERE d.IsActive = 1
        """;

    public IReadOnlyList<InterpretiveFactorRow> GetFactors()
    {
        using var connection = factory.CreateOpenConnection();
        return connection.Query<InterpretiveFactorRow>("""
            SELECT CAST(Id AS INT) AS Id, FactorCode, FactorName, Description, CAST(SortOrder AS INT) AS SortOrder
            FROM dbo.tbl_Dim_InterpretiveFactor WHERE IsActive = 1 ORDER BY SortOrder
            """).ToList();
    }

    public IReadOnlyList<InterpretiveFactorDetailRow> GetForLifeArea(int lifeAreaId)
    {
        using var connection = factory.CreateOpenConnection();
        return connection.Query<InterpretiveFactorDetailRow>(
            DetailSelect + " AND d.LifeAreaId = @LifeAreaId ORDER BY f.SortOrder, d.DisplayOrder",
            new { LifeAreaId = lifeAreaId }).ToList();
    }

    public IReadOnlyList<InterpretiveFactorDetailRow> GetForDivisionalSubject(string subjectCode)
    {
        using var connection = factory.CreateOpenConnection();
        return connection.Query<InterpretiveFactorDetailRow>(
            DetailSelect + " AND d.DivisionalSubjectCode = @SubjectCode ORDER BY f.SortOrder, d.DisplayOrder",
            new { SubjectCode = subjectCode }).ToList();
    }

    public IReadOnlyList<InterpretiveFactorDetailRow> GetForKarakaRole(int karakaRoleId)
    {
        using var connection = factory.CreateOpenConnection();
        return connection.Query<InterpretiveFactorDetailRow>(
            DetailSelect + " AND d.KarakaRoleId = @KarakaRoleId ORDER BY f.SortOrder, d.DisplayOrder",
            new { KarakaRoleId = karakaRoleId }).ToList();
    }
}
