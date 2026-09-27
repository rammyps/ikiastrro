using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.LifeMatters;

public enum LifeMatterFocusKind
{
    House,
    SpecialPoint
}

public sealed record LifeMatterSubjectRule(
    int Id,
    byte RuleSetId,
    int LifeMatterId,
    string SubjectCode,
    string ChartTypeCode,
    bool IsActive = true);

public sealed record LifeMatterKarakaRule(
    int Id,
    byte RuleSetId,
    int LifeMatterId,
    int KarakaRoleId,
    string KarakaCode,
    int DisplayOrder,
    bool IsActive = true);

public sealed record LifeMatterFocusRule(
    int Id,
    byte RuleSetId,
    int LifeMatterId,
    LifeMatterFocusKind FocusKind,
    string? ReferenceCode,
    int? HouseNumber,
    string? SpecialPointCode,
    int Priority,
    bool IsActive = true);

public sealed record ResolvedLifeMatterFocus(
    int LifeMatterId,
    LifeMatterSubjectRule? Subject,
    IReadOnlyList<LifeMatterKarakaRule> Karakas,
    IReadOnlyList<LifeMatterFocusRule> HouseAndSpecialPointFoci)
{
    public bool IsFocusStructured => Karakas.Count > 0 || HouseAndSpecialPointFoci.Count > 0;
}

/// <summary>
/// Combines the independent Subject, Karaka, and House/SpecialPoint rule sources for one
/// LifeMatter. This class is deliberately persistence- and UI-independent.
/// </summary>
public sealed class LifeMatterFocusResolver
{
    public ResolvedLifeMatterFocus Resolve(
        byte ruleSetId,
        int lifeMatterId,
        IEnumerable<LifeMatterSubjectRule> subjects,
        IEnumerable<LifeMatterKarakaRule> karakas,
        IEnumerable<LifeMatterFocusRule> foci)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lifeMatterId);

        var matchingSubjects = subjects
            .Where(x => x.IsActive && x.RuleSetId == ruleSetId && x.LifeMatterId == lifeMatterId)
            .ToArray();

        if (matchingSubjects.Length > 1)
            throw new InvalidOperationException(
                $"LifeMatter {lifeMatterId} has {matchingSubjects.Length} active Subject mappings in RuleSet {ruleSetId}; expected at most one.");

        var matchingKarakas = karakas
            .Where(x => x.IsActive && x.RuleSetId == ruleSetId && x.LifeMatterId == lifeMatterId)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .ToArray();

        var matchingFoci = foci
            .Where(x => x.IsActive && x.RuleSetId == ruleSetId && x.LifeMatterId == lifeMatterId)
            .Select(Validate)
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.Id)
            .ToArray();

        return new ResolvedLifeMatterFocus(
            lifeMatterId,
            matchingSubjects.SingleOrDefault(),
            matchingKarakas,
            matchingFoci);
    }

    public static ZodiacName ResolveHouseSign(ZodiacName ascendantSign, int houseNumber)
    {
        if (houseNumber is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(houseNumber), houseNumber, "House number must be from 1 through 12.");

        return Enum.GetValues<ZodiacName>()
            .Single(sign => AstroMath.CountFromSignToSign(ascendantSign, sign) == houseNumber);
    }

    private static LifeMatterFocusRule Validate(LifeMatterFocusRule focus)
    {
        switch (focus.FocusKind)
        {
            case LifeMatterFocusKind.House when
                !string.IsNullOrWhiteSpace(focus.ReferenceCode) &&
                focus.HouseNumber is >= 1 and <= 12 &&
                focus.SpecialPointCode is null:
                return focus;

            case LifeMatterFocusKind.SpecialPoint when
                focus.ReferenceCode is null &&
                focus.HouseNumber is null &&
                IsCanonicalSpecialPointCode(focus.SpecialPointCode):
                return focus;

            case LifeMatterFocusKind.SpecialPoint when
                string.Equals(focus.SpecialPointCode, "UL", StringComparison.OrdinalIgnoreCase):
                throw new InvalidOperationException("UL is a display alias. Store the canonical SpecialPointCode A12.");

            default:
                throw new InvalidOperationException($"LifeMatterFocus rule {focus.Id} has an invalid {focus.FocusKind} payload.");
        }
    }

    private static bool IsCanonicalSpecialPointCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        if (code == "AL")
            return true;

        // "GA_AK" - a dynamic sentinel, not a fixed graha: resolves per-person at render time to
        // whichever planet is that chart's own Atmakaraka, then that planet's own GA_<Planet>
        // Graha Arudha point (see LifeMatters.razor's SpecialPointsBySign). Unlike CharaKarakaCode
        // on the older tbl_Rule_LifeMatterReference table, tbl_Rule_LifeMatterFocus has no
        // separate "varies by person" column, so the sentinel lives directly in SpecialPointCode.
        if (code == "GA_AK")
            return true;

        return code.Length is 2 or 3 &&
               code[0] == 'A' &&
               int.TryParse(code.AsSpan(1), out var house) &&
               house is >= 2 and <= 12;
    }
}
