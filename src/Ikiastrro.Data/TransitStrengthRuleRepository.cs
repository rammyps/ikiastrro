using Dapper;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Engines.Transits;

namespace Ikiastrro.Data;

/// <summary>tbl_Rule_VimsopakaWeight (migration 157, BPHS) and tbl_Rule_GocharaVedha (migration 158,
/// PVR ch.26.3) — the rule rows <see cref="VimsopakaCalculator"/> and
/// <see cref="GocharaVedhaCalculator"/> take, read by Astro Facts.</summary>
public sealed class TransitStrengthRuleRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public TransitStrengthRuleRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public IReadOnlyList<VimsopakaWeight> GetVimsopakaWeights(int ruleSetId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<VimsopakaWeight>("""
            SELECT SchemeCode, VargaChartType, Weight
            FROM dbo.tbl_Rule_VimsopakaWeight
            WHERE RuleSetId = @ruleSetId AND IsActive = 1
            ORDER BY SchemeCode, Id
            """, new { ruleSetId }).ToList();
    }

    public IReadOnlyList<GocharaVedhaRule> GetGocharaVedhaRules(int ruleSetId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<(string Planet, byte Good, byte Vedha, string? Excluded)>("""
            SELECT p.PlanetName, r.AuspiciousHouse, r.VedhaHouse, x.PlanetName
            FROM dbo.tbl_Rule_GocharaVedha r
            JOIN dbo.tbl_Planets p ON p.Id = r.TransitPlanetId
            LEFT JOIN dbo.tbl_Planets x ON x.Id = r.ExcludedObstructorPlanetId
            WHERE r.RuleSetId = @ruleSetId
            ORDER BY r.TransitPlanetId, r.AuspiciousHouse
            """, new { ruleSetId })
            .Select(r => new GocharaVedhaRule(Enum.Parse<PlanetName>(r.Planet), r.Good, r.Vedha,
                r.Excluded is null ? null : Enum.Parse<PlanetName>(r.Excluded)))
            .ToList();
    }
}
