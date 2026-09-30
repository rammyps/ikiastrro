namespace Ikiastrro.Web.Components.LifeMatters;

/// <summary>
/// Shareable Life Matters links: <c>/life-matters/{id}?matter=WEALTH_02&amp;lagna=MO&amp;chart=D9</c>
/// (docs/ui/components/specs_life_matters_page.md "Deep links"). <c>matter</c> is a
/// tbl_Dim_LifeMatter code, <c>lagna</c> a perspective's tag or its tbl_Dim_HouseReference code,
/// <c>chart</c> a chart code. Defaults are left out of the URL: Lagna, and the matter's own chart.
/// </summary>
public static class LifeMatterLink
{
    public static string Url(int personId, string? matterCode, string referenceCode, string? chartOverride)
    {
        var query = new List<string>();
        if (!string.IsNullOrEmpty(matterCode)) query.Add($"matter={Uri.EscapeDataString(matterCode)}");
        if (referenceCode != "LAGNA") query.Add($"lagna={Uri.EscapeDataString(LifeMatterQuestions.Get(referenceCode).Tag)}");
        if (!string.IsNullOrEmpty(chartOverride)) query.Add($"chart={Uri.EscapeDataString(chartOverride)}");
        return $"/life-matters/{personId}" + (query.Count == 0 ? "" : "?" + string.Join("&", query));
    }

    /// <summary>A perspective from its tag ("MO") or reference code ("CHANDRA_LAGNA"), any case;
    /// null when it names none.</summary>
    public static string? ResolveLagna(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null
        : LifeMatterQuestions.All.FirstOrDefault(q =>
            string.Equals(q.Tag, value, StringComparison.OrdinalIgnoreCase)
            || string.Equals(q.ReferenceCode, value, StringComparison.OrdinalIgnoreCase))?.ReferenceCode;

    /// <summary>The generated chart a value names ("d10" → "D10"); null when it names none.</summary>
    public static string? ResolveChart(string? value, IEnumerable<string> generatedCharts) =>
        string.IsNullOrWhiteSpace(value) ? null
        : generatedCharts.FirstOrDefault(c => string.Equals(c, value, StringComparison.OrdinalIgnoreCase));
}
