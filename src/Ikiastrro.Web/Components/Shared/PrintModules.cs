namespace Ikiastrro.Web.Components.Shared;

/// <summary>One printable module: a URL code and the label shown in the picker and on the page.</summary>
public sealed record PrintModule(string Code, string Label);

/// <summary>
/// The print report's module catalog and its URL (<c>/print/{id}?af=natal,yogas&amp;ki=foundation,CAREER</c>).
/// Astro Facts modules are its master tabs, in rail order (AstroFacts.MasterSteps); Key Inference
/// modules are the chart foundation plus one per life area, whose codes come from
/// tbl_Rule_LifeMatterReference. Saved Charts' Print picker builds the URL; PrintReport reads it.
/// </summary>
public static class PrintModules
{
    public const string Foundation = "foundation";

    /// <summary>Index = AstroFacts' <c>_masterTab</c>.</summary>
    public static readonly IReadOnlyList<PrintModule> AstroFacts =
    [
        new("natal", "Natal"),
        new("transit", "Transit"),
        new("strength", "Strength"),
        new("spllagnas", "Spl Lagnas"),
        new("yogas", "Yogas"),
        new("vargas", "Vargas"),
        new("allcharts", "All Charts"),
    ];

    /// <summary>Foundation, then one module per life area, labelled as Key Inference's area pills.</summary>
    public static IReadOnlyList<PrintModule> KeyInference(IEnumerable<Ikiastrro.Data.LifeMatterCategoryRow> areas) =>
    [
        new(Foundation, "Foundation"),
        .. areas.Select(a => new PrintModule(a.CategoryCode, LifeMatters.LifeMatterQuestions.AreaLabel(a.CategoryCode, a.CategoryName))),
    ];

    /// <summary>Compatibility modules: one per partner, the code being the partner's saved-person id.
    /// Offered for the person's recorded spouse(s); the report compares the printed person with each.</summary>
    public static PrintModule Partner(int partnerId, string name) => new(partnerId.ToString(), name);

    public static string Url(int personId, IEnumerable<string> astroFacts, IEnumerable<string> keyInference,
        IEnumerable<string>? compatibility = null)
    {
        var url = $"/print/{personId}?af={Uri.EscapeDataString(string.Join(',', astroFacts))}&ki={Uri.EscapeDataString(string.Join(',', keyInference))}";
        var cp = string.Join(',', compatibility ?? []);
        return cp.Length == 0 ? url : $"{url}&cp={Uri.EscapeDataString(cp)}";
    }

    /// <summary>A comma list from the URL; null or blank means none.</summary>
    public static IReadOnlySet<string> Parse(string? list) =>
        (list ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
}
