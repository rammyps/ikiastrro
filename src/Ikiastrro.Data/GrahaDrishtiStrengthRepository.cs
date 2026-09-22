using Dapper;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Relationships;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Data;

/// <summary>Persists the D1/D9/D10 sphuta Graha-drishti matrix. Discrete aspect rows remain
/// owned by ChartAspectsRepository; the nullable ordinal here is calculation metadata only.</summary>
public sealed class GrahaDrishtiStrengthRepository
{
    private static readonly HashSet<string> SupportedChartTypes =
        new(StringComparer.OrdinalIgnoreCase) { "D1", "D9", "D10" };

    private readonly SqlConnectionFactory _connectionFactory;
    public GrahaDrishtiStrengthRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public void DeleteByBirthDetailId(int birthDetailId)
    {
        const string sql = """
            DELETE FROM dbo.tbl_Fact_GrahaDrishtiStrengths
            WHERE ChartResultId IN (SELECT Id FROM dbo.tbl_ChartResults WHERE BirthDetailId = @BirthDetailId)
            """;
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(sql, new { BirthDetailId = birthDetailId });
    }

    public void Replace(int chartResultId, int ruleSetId, ChartAnalysisInput input)
    {
        if (!SupportedChartTypes.Contains(input.ChartType)) return;

        var points = input.Planets
            .Where(p => p.PointKind == "Graha" && Longitude(p).HasValue)
            .ToList();
        var aspecting = points
            .Where(p => Enum.TryParse<PlanetName>(p.Planet, true, out _))
            .ToList();

        using var connection = _connectionFactory.CreateOpenConnection();
        using var transaction = connection.BeginTransaction();
        connection.Execute(
            "DELETE dbo.tbl_Fact_GrahaDrishtiStrengths WHERE ChartResultId = @ChartResultId",
            new { ChartResultId = chartResultId }, transaction);

        const string sql = """
            INSERT dbo.tbl_Fact_GrahaDrishtiStrengths
                (ChartResultId, RuleSetId, ChartTypeId, AspectingPlanetId,
                 AspectedPointKind, AspectedPointKey, AspectedPlanetId,
                 AspectingLongitudeDegrees, AspectedLongitudeDegrees, DirectedSeparationDegrees,
                 OrdinaryVirupas, SpecialVirupas, TotalVirupas, StrengthPercentage,
                 DiscreteAspectHouse, SourceRefCode)
            VALUES
                (@ChartResultId, @RuleSetId, (SELECT ChartTypeId FROM dbo.tbl_ChartResults WHERE Id = @ChartResultId), @AspectingPlanetId,
                 @AspectedPointKind, @AspectedPointKey, @AspectedPlanetId,
                 @AspectingLongitudeDegrees, @AspectedLongitudeDegrees, @DirectedSeparationDegrees,
                 @OrdinaryVirupas, @SpecialVirupas, @TotalVirupas, @StrengthPercentage,
                 @DiscreteAspectHouse, 'SRC_PVR_INTEGRATED')
            """;

        foreach (var source in aspecting)
        {
            var sourcePlanet = Enum.Parse<PlanetName>(source.Planet, true);
            var sourceLongitude = Longitude(source)!.Value;
            foreach (var target in points.Where(p => !ReferenceEquals(p, source)))
            {
                var targetLongitude = Longitude(target)!.Value;
                var result = GrahaDrishtiStrengthCalculator.Calculate(sourcePlanet, sourceLongitude, targetLongitude);
                var targetIsPlanet = Enum.TryParse<PlanetName>(target.Planet, true, out var targetPlanet);
                connection.Execute(sql, new
                {
                    ChartResultId = chartResultId,
                    RuleSetId = ruleSetId,

                    AspectingPlanetId = AstroIds.PlanetId(sourcePlanet),
                    AspectedPointKind = targetIsPlanet ? "Graha" : "Lagna",
                    AspectedPointKey = target.Planet,
                    AspectedPlanetId = targetIsPlanet ? AstroIds.PlanetId(targetPlanet) : (int?)null,
                    AspectingLongitudeDegrees = sourceLongitude,
                    AspectedLongitudeDegrees = targetLongitude,
                    result.DirectedSeparationDegrees,
                    result.OrdinaryVirupas,
                    result.SpecialVirupas,
                    result.TotalVirupas,
                    StrengthPercentage = result.Percentage,
                    result.DiscreteAspectHouse
                }, transaction);
            }
        }

        transaction.Commit();
    }

    private static double? Longitude(PlanetPosition point) =>
        point.VargaLongitudeDegrees ?? point.NirayanaLongitudeDegrees;
}
