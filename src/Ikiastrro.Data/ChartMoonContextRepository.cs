using Dapper;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Data;

/// <summary>
/// dbo.vw_ChartMoonContext — one row of D1 Moon/tithi context per person. Read-only view, so
/// (like <see cref="ChartHouseLordInterpretationRepository"/>) there's no Insert/Delete
/// companion here.
/// </summary>
public class ChartMoonContextRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public ChartMoonContextRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>Null when D1 isn't computed for this person yet.</summary>
    public MoonContext? GetByBirthDetailId(int birthDetailId)
    {
        const string sql = "SELECT * FROM dbo.vw_ChartMoonContext WHERE BirthDetailId = @BirthDetailId";
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.QuerySingleOrDefault<MoonContext>(sql, new { BirthDetailId = birthDetailId });
    }
}
