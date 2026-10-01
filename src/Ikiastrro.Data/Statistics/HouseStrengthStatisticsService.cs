using Dapper;
using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Data.Statistics;

/// <summary>One saved statistics row — a sign of one chart — as it is written to
/// tbl_Fact_HouseStrengthStatistics (migration 156).</summary>
public sealed record SavedHouseStatistics(int ChartResultId, string ChartType, HouseStatistics Stats);

/// <summary>Reads and writes tbl_Fact_HouseStrengthStatistics (migration 156). Delete-then-insert
/// per person; the FK to tbl_ChartResults has no cascade, so every path that deletes chart
/// results calls <see cref="DeleteByBirthDetailId"/> first.</summary>
public sealed class HouseStrengthStatisticsRepository(SqlConnectionFactory connectionFactory)
{
    public void DeleteByBirthDetailId(int birthDetailId)
    {
        using var connection = connectionFactory.CreateOpenConnection();
        connection.Execute("""
            DELETE FROM dbo.tbl_Fact_HouseStrengthStatistics
            WHERE ChartResultId IN (SELECT Id FROM dbo.tbl_ChartResults WHERE BirthDetailId = @birthDetailId)
            """, new { birthDetailId });
    }

    public void InsertAll(int ruleSetId, IEnumerable<SavedHouseStatistics> rows)
    {
        var list = rows.ToList();
        if (list.Count == 0) return;
        using var connection = connectionFactory.CreateOpenConnection();
        connection.Execute("""
            INSERT dbo.tbl_Fact_HouseStrengthStatistics
                (ChartResultId, RuleSetId, SignId, HouseFromLagna, LordPlanetId, SavBindus, LordBavBindus,
                 BhavaBalaRupas, IndependentBhavaRupas, IndependentBhavaZ, LordShadbalaPercent, LordAmsabalaPercent,
                 ArgalaHolds, ArgalaContested, ArgalaObstructed, Capacity, Consistency, Context, StrengthPercent)
            VALUES
                (@ChartResultId, @RuleSetId, @SignId, @HouseFromLagna, @LordPlanetId, @SavBindus, @LordBavBindus,
                 @BhavaBalaRupas, @IndependentBhavaRupas, @IndependentBhavaZ, @LordShadbalaPercent, @LordAmsabalaPercent,
                 @ArgalaHolds, @ArgalaContested, @ArgalaObstructed, @Capacity, @Consistency, @Context, @StrengthPercent)
            """, list.Select(r => new
        {
            r.ChartResultId,
            RuleSetId = ruleSetId,
            SignId = AstroIds.SignId(Enum.Parse<ZodiacName>(r.Stats.Sign)),
            r.Stats.HouseFromLagna,
            LordPlanetId = AstroIds.PlanetId(Enum.Parse<PlanetName>(r.Stats.LordPlanet)),
            r.Stats.SavBindus,
            r.Stats.LordBavBindus,
            r.Stats.BhavaBalaRupas,
            r.Stats.IndependentBhavaRupas,
            IndependentBhavaZ = r.Stats.IndependentBhavaZ is { } z ? Math.Round((decimal)z, 4) : (decimal?)null,
            r.Stats.LordShadbalaPercent,
            r.Stats.LordAmsabalaPercent,
            ArgalaHolds = r.Stats.Argala.Holds,
            ArgalaContested = r.Stats.Argala.Contested,
            ArgalaObstructed = r.Stats.Argala.Obstructed,
            r.Stats.Capacity,
            r.Stats.Consistency,
            r.Stats.Context,
            r.Stats.StrengthPercent
        }));
    }
}

/// <summary>
/// Saves one person's Key Inference strength statistics — every generated chart × 12 signs — from
/// the same <see cref="LifeMatterStatistics"/> the Key Inference page uses, reading only
/// already-persisted facts (Ashtakavarga, Bhava Bala + components, Ṣaḍbala, Amsabala, Argala).
/// Axes are read for the sign's lord only (no matter, so no kārakas). Run after chart generation.
/// </summary>
public sealed class HouseStrengthStatisticsService(
    ChartResultsRepository chartResults,
    ChartKeyDetailsRepository keyDetails,
    AshtakavargaRepository ashtakavarga,
    BhavaStrengthRepository bhavaStrength,
    PlanetaryStrengthRepository planetaryStrength,
    AmsabalaRepository amsabala,
    ArgalaFactRepository argalaFacts,
    HouseStrengthStatisticsRepository statistics,
    RuleSetRepository ruleSets)
{
    private static readonly string[] Signs = Enum.GetNames<ZodiacName>();

    /// <summary>Removes one person's rows — call before their chart results are deleted.</summary>
    public void Delete(int birthDetailId) => statistics.DeleteByBirthDetailId(birthDetailId);

    /// <summary>Rebuilds every row for one person; returns the number of rows written.</summary>
    public int Recompute(int birthDetailId)
    {
        var rows = Build(birthDetailId);
        statistics.DeleteByBirthDetailId(birthDetailId);
        statistics.InsertAll(ruleSets.GetActive().Id, rows);
        return rows.Count;
    }

    public IReadOnlyList<SavedHouseStatistics> Build(int birthDetailId)
    {
        var charts = chartResults.GetByBirthDetailId(birthDetailId)
            .Where(c => c.CalculationKind == "PositionChart")
            .ToList();
        var keyDetailsByChart = keyDetails.GetByBirthDetailId(birthDetailId).ToLookup(k => k.ChartResultId);
        var av = ashtakavarga.GetByBirthDetailId(birthDetailId);
        var bhava = bhavaStrength.GetSummaryByBirthDetailId(birthDetailId);
        var bhavaComponents = bhavaStrength.GetComponentsByBirthDetailId(birthDetailId);
        var shadbala = planetaryStrength.GetSummaryByBirthDetailId(birthDetailId);
        var amsa = amsabala.GetByBirthDetailId(birthDetailId);
        var argala = argalaFacts.GetByBirthDetailId(birthDetailId);

        var rows = new List<SavedHouseStatistics>();
        foreach (var chart in charts)
        {
            var grahas = keyDetailsByChart[chart.Id].Where(k => k.PointKind == "Graha").ToList();
            var ascendant = grahas.FirstOrDefault(k => k.Planet == "Ascendant")?.Sign;
            if (ascendant is null) continue;

            var stats = new LifeMatterStatistics(chart.ChartType, ascendant, av, bhava, shadbala,
                ArgalaFacts.ForChart(argala, chart.ChartType, ascendant, grahas), bhavaComponents, amsa);
            rows.AddRange(Signs.Select(sign => new SavedHouseStatistics(chart.Id, chart.ChartType, stats.ForSign(sign))));
        }
        return rows;
    }
}
