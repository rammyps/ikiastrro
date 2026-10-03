using Ikiastrro.Data.Statistics;

namespace Ikiastrro.Web.Components.LifeMatters;

public static class PopulationEvidenceText
{
    public static string Heading(PopulationEvidenceSnapshot snapshot) =>
        snapshot.IsAvailable ? "Population comparison" : "Population comparison unavailable";

    public static string Message(PopulationEvidenceSnapshot snapshot) => snapshot.AvailabilityCode switch
    {
        "NOT_ENROLLED" => "This saved chart is not enrolled for statistical use.",
        "NOT_ELIGIBLE" => "This chart is not eligible for the statistical reference population.",
        "NO_COMPLETED_RUN" => "No completed statistical comparison run is available.",
        "NO_COMPARISONS" when snapshot.EligiblePeople < 30 =>
            $"Insufficient reference data — {snapshot.EligiblePeople} eligible people; at least 30 are required.",
        "NO_COMPARISONS" => "No compatible comparison exists for this chart and dataset version.",
        _ => "Population evidence is available."
    };

    public static string Sufficiency(PopulationComparison comparison) => comparison.SufficiencyCode switch
    {
        "INSUFFICIENT" => "Insufficient reference data",
        "EXPLORATORY" => "Exploratory — small reference population",
        "INCOMPLETE" => "Incomplete reference data",
        _ => "Reference population"
    };

    public static string Percentile(decimal? value) =>
        value is null ? "Not published" : $"{value.Value:0.#}th percentile";
}
