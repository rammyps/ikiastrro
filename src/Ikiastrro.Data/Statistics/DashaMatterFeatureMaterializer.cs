using Dapper;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dasha;

namespace Ikiastrro.Data.Statistics;

/// <summary>
/// Writes the <c>KI_DASHA_MATTER_V1</c> natal feature rows (migration 178) for every ELIGIBLE analytics subject from
/// charts already stored. The dasha-matter rules live in Core, so the rows cannot be derived in SQL; Python then
/// describes them. Delete-then-insert per subject keeps a re-run idempotent.
/// </summary>
public sealed class DashaMatterFeatureMaterializer(
    SqlConnectionFactory connectionFactory,
    ChartResultsRepository chartResults,
    ChartKeyDetailsRepository keyDetails)
{
    private sealed record SubjectRow(Guid SubjectKey, int BirthDetailId);

    /// <summary>Pure: every PVR and extended rule for one person's charts. The Atma Karaka comes from the stored D1 rows.</summary>
    public static IReadOnlyList<DashaMatterFeature> BuildFor(
        IReadOnlyDictionary<string, DashaMatterChart> charts, PlanetName? atmaKaraka) =>
        DashaMatterFeatures.Build([.. DashaMatters.Evaluate(charts, atmaKaraka), .. DashaMatters.EvaluateExtended(charts)]);

    public (int Subjects, int Rows) Materialize()
    {
        using var connection = connectionFactory.CreateOpenConnection();
        var subjects = connection.Query<SubjectRow>("""
            SELECT SubjectKey, BirthDetailId
            FROM dbo.tbl_Dim_AnalyticsSubjects
            WHERE ResearchUseStatus = 'ELIGIBLE'
              AND SubjectClassification IN ('RESEARCH', 'PERSONAL')
              AND WithdrawnAtUtc IS NULL
            """).ToList();

        var rows = 0;
        foreach (var subject in subjects)
        {
            var results = chartResults.GetByBirthDetailId(subject.BirthDetailId);
            var charts = LifeMatterTimingDataAdapter.BuildCharts(results, keyDetails.GetByChartResultId);
            var d1 = results.FirstOrDefault(r => r.ChartType == "D1");
            var ak = d1 is null ? null : keyDetails.GetByChartResultId(d1.Id)
                .FirstOrDefault(k => k.PointKind == "Graha" && k.CharaKaraka == "AK")?.Planet;
            var features = BuildFor(charts, ak is not null && Enum.TryParse<PlanetName>(ak, out var p) ? p : null);

            using var transaction = connection.BeginTransaction();
            connection.Execute("""
                DELETE FROM dbo.tbl_Fact_AnalyticsDashaMatterFeatures
                WHERE SubjectKey = @SubjectKey AND FeatureContractVersion = @contract
                """, new { subject.SubjectKey, contract = DashaMatterFeatures.ContractVersion }, transaction);
            connection.Execute("""
                INSERT dbo.tbl_Fact_AnalyticsDashaMatterFeatures
                    (SubjectKey, RuleNumber, Varga, ScopeKind, HouseNumber, KarakaCode,
                     TargetCount, MissingReasonCode, FeatureContractVersion)
                VALUES (@SubjectKey, @RuleNumber, @Varga, @ScopeKind, @House, @KarakaCode,
                        @TargetCount, @MissingReasonCode, @Contract)
                """, features.Select(f => new
                {
                    subject.SubjectKey, f.RuleNumber, f.Varga, f.ScopeKind, f.House, f.KarakaCode,
                    f.TargetCount, f.MissingReasonCode, Contract = DashaMatterFeatures.ContractVersion
                }), transaction);
            transaction.Commit();
            rows += features.Count;
        }
        return (subjects.Count, rows);
    }
}
