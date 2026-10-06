using Dapper;

namespace Ikiastrro.Data.Statistics;

public sealed record AnalyticsSubjectRecord(
    int BirthDetailId,
    string ResearchUseStatus,
    string SubjectClassification,
    string BirthTimeQuality,
    DateTime? ConsentRecordedAtUtc,
    DateTime? WithdrawnAtUtc);

public sealed record AnalyticsSubjectUpdate(
    int BirthDetailId,
    string ResearchUseStatus,
    string SubjectClassification,
    string BirthTimeQuality,
    bool ExplicitPermissionConfirmed);

/// <summary>Explicit administration of research eligibility. No chart is enrolled by default.</summary>
public sealed class AnalyticsSubjectRepository(SqlConnectionFactory connectionFactory)
{
    private static readonly HashSet<string> Statuses = ["PENDING", "ELIGIBLE", "EXCLUDED", "WITHDRAWN"];
    private static readonly HashSet<string> Classifications = ["RESEARCH", "PERSONAL", "TEST", "DEMO", "SYNTHETIC"];
    private static readonly HashSet<string> Qualities = ["UNKNOWN", "APPROXIMATE", "RECORDED", "RECTIFIED"];

    public IReadOnlyDictionary<int, AnalyticsSubjectRecord> GetAll()
    {
        using var connection = connectionFactory.CreateOpenConnection();
        return connection.Query<AnalyticsSubjectRecord>("""
            SELECT BirthDetailId, ResearchUseStatus, SubjectClassification, BirthTimeQuality,
                   ConsentRecordedAtUtc, WithdrawnAtUtc
            FROM dbo.tbl_Dim_AnalyticsSubjects
            """).ToDictionary(row => row.BirthDetailId);
    }

    public void Save(AnalyticsSubjectUpdate update)
    {
        Validate(update);
        using var connection = connectionFactory.CreateOpenConnection();
        using var transaction = connection.BeginTransaction();
        var subjectKey = connection.ExecuteScalar<Guid?>("""
            SELECT SubjectKey FROM dbo.tbl_Dim_AnalyticsSubjects
            WHERE BirthDetailId = @BirthDetailId
            """, update, transaction);
        if (subjectKey is { } key)
        {
            connection.Execute("""
                DELETE FROM dbo.tbl_Fact_LifeMatterStatisticalComparisons WHERE SubjectKey = @key
                """, new { key }, transaction);
            connection.Execute("""
                DELETE FROM dbo.tbl_Fact_StatisticalComparisons WHERE SubjectKey = @key
                """, new { key }, transaction);
        }

        connection.Execute("""
            MERGE dbo.tbl_Dim_AnalyticsSubjects AS target
            USING (SELECT @BirthDetailId AS BirthDetailId) AS source
               ON target.BirthDetailId = source.BirthDetailId
            WHEN MATCHED THEN UPDATE SET
                ResearchUseStatus = @ResearchUseStatus,
                SubjectClassification = @SubjectClassification,
                BirthTimeQuality = @BirthTimeQuality,
                ConsentRecordedAtUtc = CASE
                    WHEN @ResearchUseStatus = 'ELIGIBLE' THEN COALESCE(target.ConsentRecordedAtUtc, SYSUTCDATETIME())
                    ELSE target.ConsentRecordedAtUtc END,
                WithdrawnAtUtc = CASE WHEN @ResearchUseStatus = 'WITHDRAWN' THEN SYSUTCDATETIME() ELSE NULL END,
                UpdatedAtUtc = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                (BirthDetailId, ResearchUseStatus, SubjectClassification, BirthTimeQuality,
                 ConsentRecordedAtUtc, WithdrawnAtUtc)
            VALUES
                (@BirthDetailId, @ResearchUseStatus, @SubjectClassification, @BirthTimeQuality,
                 CASE WHEN @ResearchUseStatus = 'ELIGIBLE' THEN SYSUTCDATETIME() ELSE NULL END,
                 CASE WHEN @ResearchUseStatus = 'WITHDRAWN' THEN SYSUTCDATETIME() ELSE NULL END);
            """, update, transaction);
        transaction.Commit();
    }

    public void DeleteByBirthDetailId(int birthDetailId)
    {
        using var connection = connectionFactory.CreateOpenConnection();
        using var transaction = connection.BeginTransaction();
        connection.Execute("""
            DELETE comparison
            FROM dbo.tbl_Fact_LifeMatterStatisticalComparisons comparison
            JOIN dbo.tbl_Dim_AnalyticsSubjects subject ON subject.SubjectKey = comparison.SubjectKey
            WHERE subject.BirthDetailId = @birthDetailId
            """, new { birthDetailId }, transaction);
        connection.Execute("""
            DELETE comparison
            FROM dbo.tbl_Fact_StatisticalComparisons comparison
            JOIN dbo.tbl_Dim_AnalyticsSubjects subject ON subject.SubjectKey = comparison.SubjectKey
            WHERE subject.BirthDetailId = @birthDetailId
            """, new { birthDetailId }, transaction);
        connection.Execute("""
            DELETE FROM dbo.tbl_Dim_AnalyticsSubjects WHERE BirthDetailId = @birthDetailId
            """, new { birthDetailId }, transaction);
        transaction.Commit();
    }

    public static void Validate(AnalyticsSubjectUpdate update)
    {
        if (!Statuses.Contains(update.ResearchUseStatus))
            throw new ArgumentException("Unknown research-use status.");
        if (!Classifications.Contains(update.SubjectClassification))
            throw new ArgumentException("Unknown subject classification.");
        if (!Qualities.Contains(update.BirthTimeQuality))
            throw new ArgumentException("Unknown birth-time quality.");
        if (update.ResearchUseStatus == "ELIGIBLE" &&
            (update.SubjectClassification is not ("RESEARCH" or "PERSONAL") ||
             !update.ExplicitPermissionConfirmed))
            throw new ArgumentException("Eligibility requires a personal or research classification and explicit permission.");
    }
}
