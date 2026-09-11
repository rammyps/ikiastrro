using Dapper;
using Ikiastrro.Core.Engines.Panchanga;

namespace Ikiastrro.Data;

/// <summary>
/// Persists Tithi / Karana / Nitya Yoga / Vedic Weekday / Hora Lord for one chart. Written on
/// the D1 <c>tbl_ChartResults</c> row only, mirroring the strength / vargottama / Ashtakavarga
/// facts. Schema: db/081_create_panchanga_schema.sql.
/// </summary>
public sealed class PanchangaRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public PanchangaRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public void Insert(int chartResultId, int ruleSetId, PanchangaResult result)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(
            """
            INSERT dbo.tbl_Chart_Panchanga
                (ChartResultId, RuleSetId, SunriseUtc, SunsetUtc, NextSunriseUtc, IsNightBirth,
                 JanmaGhatis, SunMoonDeltaDegrees, TithiId, TithiPercentRemaining,
                 KaranaId, KaranaPercentRemaining, SunMoonSumDegrees, NityaYogaId,
                 NityaYogaPercentRemaining, VedicWeekdayId, HoraLordPlanetId)
            VALUES
                (@ChartResultId, @RuleSetId, @SunriseUtc, @SunsetUtc, @NextSunriseUtc, @IsNightBirth,
                 @JanmaGhatis, @SunMoonDeltaDegrees, @TithiId, @TithiPercentRemaining,
                 @KaranaId, @KaranaPercentRemaining, @SunMoonSumDegrees, @NityaYogaId,
                 @NityaYogaPercentRemaining, @VedicWeekdayId, @HoraLordPlanetId)
            """,
            new
            {
                ChartResultId = chartResultId,
                RuleSetId = ruleSetId,
                SunriseUtc = result.SunriseLocal.UtcDateTime,
                SunsetUtc = result.SunsetLocal.UtcDateTime,
                NextSunriseUtc = result.NextSunriseLocal.UtcDateTime,
                result.IsNightBirth,
                result.JanmaGhatis,
                result.SunMoonDeltaDegrees,
                result.TithiId,
                result.TithiPercentRemaining,
                result.KaranaId,
                result.KaranaPercentRemaining,
                result.SunMoonSumDegrees,
                result.NityaYogaId,
                result.NityaYogaPercentRemaining,
                result.VedicWeekdayId,
                result.HoraLordPlanetId,
            });
    }

    public void DeleteByChartResultId(int chartResultId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(
            "DELETE FROM dbo.tbl_Chart_Panchanga WHERE ChartResultId = @ChartResultId",
            new { ChartResultId = chartResultId });
    }

    /// <summary>Clears every stored chart's Panchanga row for one person — the delete-first step
    /// of ChartGenerationService.GenerateAll, mirroring AshtakavargaRepository's method of the
    /// same name. tbl_Chart_Panchanga's FK to tbl_ChartResults does cascade, but GenerateAll
    /// deletes analytics tables before it deletes the tbl_ChartResults rows, so the explicit
    /// delete here (not a cascade side effect) is what actually runs.</summary>
    public void DeleteByBirthDetailId(int birthDetailId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(
            """
            DECLARE @ids TABLE (Id INT PRIMARY KEY);
            INSERT @ids SELECT Id FROM dbo.tbl_ChartResults WHERE BirthDetailId = @BirthDetailId;
            DELETE FROM dbo.tbl_Chart_Panchanga WHERE ChartResultId IN (SELECT Id FROM @ids);
            """,
            new { BirthDetailId = birthDetailId });
    }
}
