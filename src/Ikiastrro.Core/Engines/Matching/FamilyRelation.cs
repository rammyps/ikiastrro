namespace Ikiastrro.Core.Engines.Matching;

/// <summary>The kind of relative, as the family view (vw_PersonFamily) names it.</summary>
public enum FamilyKind { Spouse, Parent, Child, Grandparent, Grandchild, Sibling, Other }

/// <summary>The family layer of a relationship, relative to the person it is viewed from (migration 170):
/// Core is spouse, parents and children; Extended is grandparents and grandchildren; Lateral is siblings
/// and, later, uncles and aunts, cousins and in-laws. A layer is a property of the pair as one person sees
/// it, so it is derived from the stored edges and never stored.</summary>
public enum FamilyLayer { Core, Extended, Lateral }

/// <summary>
/// Reads a stored family role ("Wife", "Daughter", "Paternal Grandmother", "Sister", ...) as a
/// <see cref="FamilyKind"/>, turns it around for the other person, names its layer, and names the houses
/// worth examining when one person's planets are laid on the other's chart. The key-house sets are display
/// choices built on the standard house meanings (7th partner, 5th children, 3rd and 11th siblings, 4th and
/// 9th parents, 1st the self); traditions differ on parents (4th/9th vs 4th/10th), so they are shown as "key
/// houses", never as a verdict. No set is suggested for grandparents, grandchildren or other relatives: no
/// source is cited for one. Pure; no I/O.
/// </summary>
public static class FamilyRelation
{
    public static FamilyKind FromRole(string? role)
    {
        if (role is null) return FamilyKind.Other;
        if (role.Contains("Grandfather", StringComparison.Ordinal) || role.Contains("Grandmother", StringComparison.Ordinal) || role == "Grandparent")
            return FamilyKind.Grandparent;
        return role switch
        {
            "Husband" or "Wife" or "Spouse" => FamilyKind.Spouse,
            "Father" or "Mother" or "Parent" => FamilyKind.Parent,
            "Son" or "Daughter" or "Child" => FamilyKind.Child,
            "Grandson" or "Granddaughter" or "Grandchild" => FamilyKind.Grandchild,
            "Brother" or "Sister" or "Sibling" => FamilyKind.Sibling,
            _ => FamilyKind.Other,
        };
    }

    public static FamilyLayer LayerOf(FamilyKind kind) => kind switch
    {
        FamilyKind.Spouse or FamilyKind.Parent or FamilyKind.Child => FamilyLayer.Core,
        FamilyKind.Grandparent or FamilyKind.Grandchild => FamilyLayer.Extended,
        _ => FamilyLayer.Lateral,
    };

    /// <summary>How the first person is related to the second, given how the second is related to the first.</summary>
    public static FamilyKind Invert(FamilyKind kind) => kind switch
    {
        FamilyKind.Parent => FamilyKind.Child,
        FamilyKind.Child => FamilyKind.Parent,
        FamilyKind.Grandparent => FamilyKind.Grandchild,
        FamilyKind.Grandchild => FamilyKind.Grandparent,
        _ => kind,
    };

    /// <summary>The houses of one person's chart where the other's planets are most worth reading, when the
    /// other is a <paramref name="otherIs"/> to that person. Without a recorded relationship use Spouse.</summary>
    public static IReadOnlyList<int> KeyHouses(FamilyKind otherIs) => otherIs switch
    {
        FamilyKind.Spouse => [1, 5, 7, 8, 12],
        FamilyKind.Child => [1, 5],
        FamilyKind.Parent => [1, 4, 9],
        FamilyKind.Sibling => [1, 3, 11],
        _ => [1],
    };

    public static string Label(FamilyKind kind, string? sex) => kind switch
    {
        FamilyKind.Spouse => Pick(sex, "Husband", "Wife", "Spouse"),
        FamilyKind.Parent => Pick(sex, "Father", "Mother", "Parent"),
        FamilyKind.Child => Pick(sex, "Son", "Daughter", "Child"),
        FamilyKind.Grandparent => Pick(sex, "Grandfather", "Grandmother", "Grandparent"),
        FamilyKind.Grandchild => Pick(sex, "Grandson", "Granddaughter", "Grandchild"),
        FamilyKind.Sibling => Pick(sex, "Brother", "Sister", "Sibling"),
        _ => "Relative",
    };

    private static string Pick(string? sex, string male, string female, string neutral) =>
        string.Equals(sex, "Male", StringComparison.OrdinalIgnoreCase) ? male
        : string.Equals(sex, "Female", StringComparison.OrdinalIgnoreCase) ? female : neutral;
}
