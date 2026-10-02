using Ikiastrro.Core.LifeMatters;
using Ikiastrro.Data;

namespace Ikiastrro.Web.Components.LifeMatters;

/// <summary>One life matter read from a house — a Astro Facts house row's link into Key Inference.</summary>
public sealed record HouseMatterLink(string MatterCode, string MatterText, string AreaName);

/// <summary>
/// Which life matters each house of the main chart speaks for: the matters whose House focus
/// counts from the Lagna (tbl_Rule_LifeMatterFocus, ReferenceCode LAGNA). Matters read only from
/// a special lagna or a special point are left out — a house row is counted from the Lagna.
/// </summary>
public static class LifeMatterHouseIndex
{
    public static IReadOnlyDictionary<int, IReadOnlyList<HouseMatterLink>> ByHouse(
        IEnumerable<LifeMatterStepRow> steps, IEnumerable<LifeMatterFocusRule> foci)
    {
        var stepList = steps.ToList();
        var order = stepList.Select((s, i) => (s.LifeMatterId, i)).ToDictionary(x => x.LifeMatterId, x => x.i);
        var byId = stepList.ToDictionary(s => s.LifeMatterId);

        return foci
            .Where(f => f.IsActive && f.FocusKind == LifeMatterFocusKind.House && f.HouseNumber is >= 1 and <= 12
                        && (f.ReferenceCode ?? "LAGNA") == "LAGNA" && byId.ContainsKey(f.LifeMatterId))
            .GroupBy(f => f.HouseNumber!.Value)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<HouseMatterLink>)g
                    .GroupBy(f => f.LifeMatterId)
                    .Select(m => (Step: byId[m.Key], Priority: m.Min(f => f.Priority)))
                    .OrderBy(m => m.Priority).ThenBy(m => order[m.Step.LifeMatterId])
                    .Select(m => new HouseMatterLink(m.Step.LifeMatterCode, m.Step.MatterText, m.Step.CategoryName))
                    .ToList());
    }
}
