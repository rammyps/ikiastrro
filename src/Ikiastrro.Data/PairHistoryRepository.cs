using Dapper;

namespace Ikiastrro.Data;

public sealed record PairNote(int Id, string NoteText, DateTime CreatedAtUtc);

public sealed record SavedPairReport(
    int Id, int GroomId, int BrideId, string RuleSetVersion, decimal? KutaScore, decimal? KutaMax, string SummaryText, DateTime SavedAtUtc);

/// <summary>The Vimshottari lords running for a person on a date, from the stored periods (Mahadasha, Antardasha, Pratyantar).</summary>
public sealed record DashaOnDate(string? Maha, string? Antar, string? Pratyantar);

/// <summary>
/// The user's own record about a pair: the marriage date (migration 171, tbl_Person_LifeEvent), free-text
/// notes (tbl_Pair_Note) and saved report snapshots (tbl_Pair_SavedReport). Only a pair with a recorded
/// relationship can be saved as a report; an arbitrary pair is never stored, so nothing here is derived from
/// the rules and nothing goes stale (docs/architecture/compatibility_similarity.md, section 4.2).
/// </summary>
public sealed class PairHistoryRepository
{
    private const int MaxNoteLength = 1000;
    private readonly SqlConnectionFactory _connectionFactory;
    public PairHistoryRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    private static (int A, int B) Ordered(int x, int y) => x < y ? (x, y) : (y, x);

    // ---- marriage date -------------------------------------------------------------------------

    /// <summary>The recorded marriage date of the pair, or null when none is recorded.</summary>
    public DateTime? GetMarriageDate(int personId, int otherId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.QuerySingleOrDefault<DateTime?>("""
            SELECT TOP 1 EventDate FROM dbo.tbl_Person_LifeEvent
            WHERE PersonId = @PersonId AND RelatedPersonId = @OtherId AND EventType = 'MARRIAGE'
            ORDER BY EventDate
            """, new { PersonId = personId, OtherId = otherId });
    }

    /// <summary>Records the marriage date for both spouses (one row each), replacing an earlier date for the same pair.</summary>
    public void SetMarriageDate(int personId, int otherId, DateTime date)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        using var tx = connection.BeginTransaction();
        foreach (var (p, o) in new[] { (personId, otherId), (otherId, personId) })
        {
            connection.Execute("""
                DELETE dbo.tbl_Person_LifeEvent WHERE PersonId = @P AND RelatedPersonId = @O AND EventType = 'MARRIAGE';
                INSERT dbo.tbl_Person_LifeEvent (PersonId, EventType, EventDate, RelatedPersonId)
                VALUES (@P, 'MARRIAGE', @Date, @O);
                """, new { P = p, O = o, Date = date.Date }, tx);
        }
        tx.Commit();
    }

    public void ClearMarriageDate(int personId, int otherId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute("""
            DELETE dbo.tbl_Person_LifeEvent
            WHERE EventType = 'MARRIAGE'
              AND ((PersonId = @P AND RelatedPersonId = @O) OR (PersonId = @O AND RelatedPersonId = @P))
            """, new { P = personId, O = otherId });
    }

    /// <summary>The lords running for the person on the date, or null when the person has no stored dasha periods covering it.</summary>
    public DashaOnDate? DashaOn(int birthDetailId, DateTime date)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        var rows = connection.Query<(byte LevelNumber, string Lord)>("""
            SELECT LevelNumber, Lord
            FROM dbo.tbl_Chart_DashaPeriods
            WHERE ChartResultId IN (SELECT Id FROM dbo.tbl_ChartResults WHERE BirthDetailId = @Id)
              AND LevelNumber <= 3 AND StartDate <= @Date AND @Date < EndDate
            """, new { Id = birthDetailId, Date = date.Date }).ToList();
        if (rows.Count == 0) return null;
        string? At(int level) => rows.FirstOrDefault(r => r.LevelNumber == level).Lord;
        return new DashaOnDate(At(1), At(2), At(3));
    }

    // ---- notes ---------------------------------------------------------------------------------

    public IReadOnlyList<PairNote> GetNotes(int personId, int otherId)
    {
        var (a, b) = Ordered(personId, otherId);
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<PairNote>("""
            SELECT Id, NoteText, CreatedAtUtc FROM dbo.tbl_Pair_Note
            WHERE PersonAId = @A AND PersonBId = @B ORDER BY CreatedAtUtc DESC, Id DESC
            """, new { A = a, B = b }).ToList();
    }

    /// <summary>Adds a note; blank text is ignored and long text is cut at 1000 characters.</summary>
    public void AddNote(int personId, int otherId, string text)
    {
        text = (text ?? "").Trim();
        if (text.Length == 0) return;
        if (text.Length > MaxNoteLength) text = text[..MaxNoteLength];
        var (a, b) = Ordered(personId, otherId);
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute("INSERT dbo.tbl_Pair_Note (PersonAId, PersonBId, NoteText) VALUES (@A, @B, @Text)",
            new { A = a, B = b, Text = text });
    }

    public void DeleteNote(int personId, int otherId, int noteId)
    {
        var (a, b) = Ordered(personId, otherId);
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute("DELETE dbo.tbl_Pair_Note WHERE Id = @Id AND PersonAId = @A AND PersonBId = @B",
            new { Id = noteId, A = a, B = b });
    }

    // ---- saved reports -------------------------------------------------------------------------

    public IReadOnlyList<SavedPairReport> GetReports(int personId, int otherId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<SavedPairReport>("""
            SELECT Id, GroomId, BrideId, RuleSetVersion, KutaScore, KutaMax, SummaryText, SavedAtUtc
            FROM dbo.tbl_Pair_SavedReport
            WHERE (GroomId = @P AND BrideId = @O) OR (GroomId = @O AND BrideId = @P)
            ORDER BY SavedAtUtc DESC, Id DESC
            """, new { P = personId, O = otherId }).ToList();
    }

    /// <summary>True when the two people have a recorded relationship (marriage, parent, child, grandparent, sibling ...).</summary>
    public bool HasRecordedRelationship(int personId, int otherId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM dbo.vw_PersonFamily WHERE PersonId = @P AND RelativeId = @O",
            new { P = personId, O = otherId }) > 0;
    }

    /// <summary>Saves a snapshot; returns false (and stores nothing) when the pair has no recorded relationship.</summary>
    public bool SaveReport(int groomId, int brideId, string ruleSetVersion, decimal? kutaScore, decimal? kutaMax, string summaryText)
    {
        if (!HasRecordedRelationship(groomId, brideId)) return false;
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute("""
            INSERT dbo.tbl_Pair_SavedReport (GroomId, BrideId, RuleSetVersion, KutaScore, KutaMax, SummaryText)
            VALUES (@GroomId, @BrideId, @RuleSetVersion, @KutaScore, @KutaMax, @SummaryText)
            """, new { GroomId = groomId, BrideId = brideId, RuleSetVersion = ruleSetVersion, KutaScore = kutaScore, KutaMax = kutaMax, SummaryText = summaryText });
        return true;
    }

    public void DeleteReport(int personId, int otherId, int reportId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute("""
            DELETE dbo.tbl_Pair_SavedReport
            WHERE Id = @Id AND ((GroomId = @P AND BrideId = @O) OR (GroomId = @O AND BrideId = @P))
            """, new { Id = reportId, P = personId, O = otherId });
    }
}
