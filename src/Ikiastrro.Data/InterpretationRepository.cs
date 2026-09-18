using Dapper;

namespace Ikiastrro.Data;

public sealed record InterpretationRow(
    int Id, string SubjectType, string SubjectCode, string? SourceRefCode,
    string StandardText, string ShortText);

/// <summary>
/// Read + write over tbl_Content_Interpretation (db/080, source-override support added by
/// db/121). Generic (SubjectType, SubjectCode[, SourceRefCode]) key so one table serves every
/// domain, not just yogas. Editable in the UI by any user (no auth model exists in this app) —
/// Upsert is called from a confirm-gated inline editor, never automatically.
/// </summary>
public sealed class InterpretationRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public InterpretationRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public IReadOnlyList<InterpretationRow> GetBySubjectType(int ruleSetId, string subjectType)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<InterpretationRow>("""
            SELECT Id, SubjectType, SubjectCode, SourceRefCode, StandardText, ShortText
            FROM dbo.tbl_Content_Interpretation
            WHERE RuleSetId = @ruleSetId AND SubjectType = @subjectType AND IsActive = 1
            """, new { ruleSetId, subjectType }).ToList();
    }

    /// <summary>Resolves a source-specific row for subjectCode if one exists, else falls back
    /// to the generic (SourceRefCode IS NULL) row for that subject, else null.</summary>
    public static InterpretationRow? Resolve(IReadOnlyList<InterpretationRow> rows, string subjectCode, string? sourceRefCode) =>
        rows.FirstOrDefault(r => r.SubjectCode == subjectCode && r.SourceRefCode == sourceRefCode)
        ?? rows.FirstOrDefault(r => r.SubjectCode == subjectCode && r.SourceRefCode is null);

    public void Upsert(int ruleSetId, string subjectType, string subjectCode, string? sourceRefCode,
        string standardText, string shortText)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        var existingId = connection.QuerySingleOrDefault<int?>("""
            SELECT Id FROM dbo.tbl_Content_Interpretation
            WHERE RuleSetId = @ruleSetId AND SubjectType = @subjectType AND SubjectCode = @subjectCode
              AND ((@sourceRefCode IS NULL AND SourceRefCode IS NULL) OR SourceRefCode = @sourceRefCode)
            """, new { ruleSetId, subjectType, subjectCode, sourceRefCode });

        if (existingId is int id)
        {
            connection.Execute("""
                UPDATE dbo.tbl_Content_Interpretation SET StandardText = @standardText, ShortText = @shortText
                WHERE Id = @id
                """, new { id, standardText, shortText });
        }
        else
        {
            connection.Execute("""
                INSERT dbo.tbl_Content_Interpretation (RuleSetId, SubjectType, SubjectCode, SourceRefCode, StandardText, ShortText)
                VALUES (@ruleSetId, @subjectType, @subjectCode, @sourceRefCode, @standardText, @shortText)
                """, new { ruleSetId, subjectType, subjectCode, sourceRefCode, standardText, shortText });
        }
    }
}
