using Dapper;
using Ikiastrro.Core.Numerology;

namespace Ikiastrro.Data;

/// <summary>One candidate name kept on the NUMEROFACTS page, with its stored Cheiro numbers.</summary>
public sealed record NameOption(int Id, string Name, string Purpose, string? Note, int CompoundNumber, int RootNumber, DateTime CreatedAt);

/// <summary>
/// Reads and writes dbo.tbl_NameOption (migration 177). Every name is saved to the database the moment it is
/// added; the compound and root numbers are computed by <see cref="CheiroNumerology"/> and stored beside it.
/// </summary>
public sealed class NameOptionRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public NameOptionRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public IReadOnlyList<NameOption> GetAll()
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<NameOption>("""
            SELECT Id, Name, Purpose, Note, CAST(CompoundNumber AS INT) AS CompoundNumber, CAST(RootNumber AS INT) AS RootNumber, CreatedAt
            FROM dbo.tbl_NameOption
            ORDER BY Purpose, RootNumber, Name
            """).ToList();
    }

    /// <summary>Saves the name. Returns false when it has no countable letters or is already stored.</summary>
    public bool Add(string name, string purpose, string? note)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var trimmed = string.Join(' ', name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        var n = CheiroNumerology.Calculate(trimmed);
        if (n.Letters.Count == 0) return false;

        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Execute("""
            IF NOT EXISTS (SELECT 1 FROM dbo.tbl_NameOption WHERE Name = @Name)
                INSERT dbo.tbl_NameOption (Name, Purpose, Note, CompoundNumber, RootNumber)
                VALUES (@Name, @Purpose, @Note, @Compound, @Root);
            """, new
        {
            Name = trimmed,
            Purpose = string.IsNullOrWhiteSpace(purpose) ? "Business" : purpose.Trim(),
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            Compound = n.CompoundTotal,
            Root = n.RootNumber,
        }) > 0;
    }

    public void Remove(int id)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute("DELETE FROM dbo.tbl_NameOption WHERE Id = @Id", new { Id = id });
    }
}
