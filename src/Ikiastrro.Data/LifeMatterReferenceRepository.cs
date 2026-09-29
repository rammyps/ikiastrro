using Dapper;
using Ikiastrro.Core.LifeMatters;

namespace Ikiastrro.Data;

public sealed record LifeMatterStepRow(
    int LifeMatterId,
    string LifeMatterCode,
    string CategoryCode,
    string CategoryName,
    int DisplayOrder,
    string MatterText,
    string PrimaryChartsText,
    string HouseFromLagnaText,
    string KarakaText,
    string BasisCode,
    string? SourceRefCode);

public sealed record LifeMatterCategoryRow(string CategoryCode, string CategoryName, int DisplayOrder);

public sealed class LifeMatterReferenceRepository(SqlConnectionFactory factory)
{
    public IReadOnlyList<LifeMatterCategoryRow> GetCategories(byte ruleSetId)
    {
        using var connection = factory.CreateOpenConnection();
        return connection.Query<LifeMatterCategoryRow>("""
            SELECT CategoryCode, MAX(CategoryName) AS CategoryName,
                   CAST(MIN(DisplayOrder) AS INT) AS DisplayOrder
            FROM dbo.tbl_Rule_LifeMatterReference
            WHERE RuleSetId = @RuleSetId AND IsActive = 1
            GROUP BY CategoryCode
            ORDER BY MIN(Id)
            """, new { RuleSetId = ruleSetId }).ToList();
    }

    public IReadOnlyList<LifeMatterStepRow> GetSteps(byte ruleSetId, string categoryCode)
    {
        using var connection = factory.CreateOpenConnection();
        return connection.Query<LifeMatterStepRow>("""
            SELECT lm.LifeMatterId, dm.Code AS LifeMatterCode, lm.CategoryCode, lm.CategoryName,
                   CAST(lm.DisplayOrder AS INT) AS DisplayOrder, lm.MatterText, lm.PrimaryChartsText,
                   lm.HouseFromLagnaText, lm.KarakaText, lm.BasisCode, lm.SourceRefCode
            FROM dbo.tbl_Rule_LifeMatterReference lm
            JOIN dbo.tbl_Dim_LifeMatter dm ON dm.Id = lm.LifeMatterId
            WHERE lm.RuleSetId = @RuleSetId AND lm.CategoryCode = @CategoryCode AND lm.IsActive = 1
            ORDER BY lm.DisplayOrder, lm.Id
            """, new { RuleSetId = ruleSetId, CategoryCode = categoryCode }).ToList();
    }

    /// <summary>Every active step across all categories in one query — the Life Matters page's
    /// page-load snapshot, so area-level summaries don't need a GetSteps call per category.</summary>
    public IReadOnlyList<LifeMatterStepRow> GetAllSteps(byte ruleSetId)
    {
        using var connection = factory.CreateOpenConnection();
        return connection.Query<LifeMatterStepRow>("""
            SELECT lm.LifeMatterId, dm.Code AS LifeMatterCode, lm.CategoryCode, lm.CategoryName,
                   CAST(lm.DisplayOrder AS INT) AS DisplayOrder, lm.MatterText, lm.PrimaryChartsText,
                   lm.HouseFromLagnaText, lm.KarakaText, lm.BasisCode, lm.SourceRefCode
            FROM dbo.tbl_Rule_LifeMatterReference lm
            JOIN dbo.tbl_Dim_LifeMatter dm ON dm.Id = lm.LifeMatterId
            WHERE lm.RuleSetId = @RuleSetId AND lm.IsActive = 1
            ORDER BY lm.CategoryCode, lm.DisplayOrder, lm.Id
            """, new { RuleSetId = ruleSetId }).ToList();
    }
}

public sealed class DivisionalSubjectRepository(SqlConnectionFactory factory)
{
    public sealed record Row(string SubjectCode, string SubjectName, string ChartTypeCode, string? SourceRefCode);

    public IReadOnlyList<Row> GetActive()
    {
        using var connection = factory.CreateOpenConnection();
        return connection.Query<Row>("""
            SELECT s.SubjectCode, s.SubjectName, ct.Code AS ChartTypeCode, s.SourceRefCode
            FROM dbo.tbl_Dim_DivisionalSubject s
            JOIN dbo.tbl_Dim_ChartType ct ON ct.Id = s.PrimaryConfirmationChartId
            WHERE s.IsActive = 1
            ORDER BY s.SubjectCode
            """).ToList();
    }
}

public sealed class LifeMatterFocusRepository(SqlConnectionFactory factory)
{
    public IReadOnlyList<LifeMatterSubjectRule> GetSubjects(byte ruleSetId)
    {
        using var connection = factory.CreateOpenConnection();
        return connection.Query<LifeMatterSubjectRule>("""
            SELECT s.Id, s.RuleSetId, s.LifeMatterId, s.DivisionalSubjectCode AS SubjectCode,
                   ct.Code AS ChartTypeCode, s.IsActive
            FROM dbo.tbl_Rule_LifeMatterSubject s
            JOIN dbo.tbl_Dim_DivisionalSubject ds ON ds.SubjectCode = s.DivisionalSubjectCode
            JOIN dbo.tbl_Dim_ChartType ct ON ct.Id = ds.PrimaryConfirmationChartId
            WHERE s.RuleSetId = @RuleSetId AND s.IsActive = 1
            """, new { RuleSetId = ruleSetId }).ToList();
    }

    public IReadOnlyList<LifeMatterFocusRule> GetFoci(byte ruleSetId)
    {
        using var connection = factory.CreateOpenConnection();
        return connection.Query<LifeMatterFocusRule>("""
            SELECT Id, RuleSetId, LifeMatterId, FocusKind, ReferenceCode,
                   CAST(HouseNumber AS INT) AS HouseNumber, SpecialPointCode,
                   CAST(Priority AS INT) AS Priority, IsActive
            FROM dbo.tbl_Rule_LifeMatterFocus
            WHERE RuleSetId = @RuleSetId AND IsActive = 1
            ORDER BY LifeMatterId, Priority, Id
            """, new { RuleSetId = ruleSetId }).ToList();
    }

    /// <summary>Every active tbl_Dim_HouseReference row — the lagna perspectives a Focus row can
    /// count from, with their Perspective text and the vargas they apply in.</summary>
    public IReadOnlyList<HouseReferenceRow> GetHouseReferences()
    {
        using var connection = factory.CreateOpenConnection();
        return connection.Query<HouseReferenceRow>("""
            SELECT ReferenceCode, ReferenceName, Perspective, AppliesInVarga, CAST(SortOrder AS INT) AS SortOrder
            FROM dbo.tbl_Dim_HouseReference
            WHERE IsActive = 1
            ORDER BY SortOrder
            """).ToList();
    }
}

public sealed record HouseReferenceRow(string ReferenceCode, string ReferenceName, string Perspective, string AppliesInVarga, int SortOrder);

public sealed class KarakaMatterRepository(SqlConnectionFactory factory)
{
    public IReadOnlyList<LifeMatterKarakaRule> GetForRuleSet(byte ruleSetId)
    {
        using var connection = factory.CreateOpenConnection();
        return connection.Query<LifeMatterKarakaRule>("""
            SELECT km.Id, km.RuleSetId, km.LifeMatterId, km.KarakaRoleId,
                   CASE
                       WHEN kr.KarakaTypeCode = 'CHARA' THEN 'KARAKA_' + UPPER(kr.CharaKarakaCode)
                       ELSE 'GRAHA_' + UPPER(p.PlanetName)
                   END AS KarakaCode,
                   CAST(km.DisplayOrder AS INT) AS DisplayOrder, km.IsActive
            FROM dbo.tbl_Rule_KarakaMatter km
            JOIN dbo.tbl_Dim_KarakaRole kr ON kr.Id = km.KarakaRoleId
            LEFT JOIN dbo.tbl_Planets p ON p.Id = kr.FixedGrahaId
            WHERE km.RuleSetId = @RuleSetId AND km.IsActive = 1
            ORDER BY km.LifeMatterId, km.DisplayOrder, km.Id
            """, new { RuleSetId = ruleSetId }).ToList();
    }
}
