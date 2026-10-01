namespace Ikiastrro.Web.Components.LifeMatters;

/// <summary>One lagna perspective, asked of an area as a question: "How is the self from power?"
/// is the self read from Ghati Lagna. <see cref="ReferenceCode"/> is a tbl_Dim_HouseReference code;
/// the perspective's own meaning is that table's Perspective text, shown alongside.</summary>
public sealed record LifeMatterQuestion(string ReferenceCode, string Label, string Tag, string Angle)
{
    public string Ask(string theme) => Angle.Length == 0 ? $"How is {theme} overall?" : $"How is {theme} {Angle}?";

    /// <summary>The question asked of one matter: "Enemies from power?". Matters already worded as a
    /// question ("Who am I", "What I think of myself") fall back to the area's <see cref="Ask"/>.</summary>
    public string AskOf(string matterText, string areaTheme) =>
        matterText.Length == 0 || matterText.StartsWith("Who ", StringComparison.Ordinal)
            || matterText.StartsWith("What ", StringComparison.Ordinal) || matterText.StartsWith("How ", StringComparison.Ordinal)
            ? Ask(areaTheme)
            : $"{matterText} {(Angle.Length == 0 ? "overall" : Angle)}?";
}

/// <summary>Display copy for the Key Inference question layer. Areas are the tbl_Rule_LifeMatterReference
/// categories; each is asked from every lagna perspective, and its matters become sub-questions.</summary>
public static class LifeMatterQuestions
{
    public static readonly IReadOnlyList<LifeMatterQuestion> All =
    [
        new("LAGNA", "Lagna", "LAG", ""),
        new("CHANDRA_LAGNA", "Chandra Lagna", "MO", "from the mind"),
        new("RAVI_LAGNA", "Surya Lagna", "SU", "from the soul"),
        new("ARUDHA_LAGNA", "Arudha Lagna", "AL", "as the world sees it"),
        new("PAAKA_LAGNA", "Paaka Lagna", "PAAKA", "from the body"),
        new("KARAKAMSA_LAGNA", "Karakamsa Lagna", "KL", "from the inner self"),
        new("HORA_LAGNA", "Hora Lagna", "HL", "from wealth"),
        new("GHATI_LAGNA", "Ghati Lagna", "GL", "from power"),
        new("SREE_LAGNA", "Sree Lagna", "SL", "from prosperity"),
        new("INDU_LAGNA", "Indu Lagna", "IL", "from wealth-yielding capacity"),
        new("PRANAPADA_LAGNA", "Pranapada Lagna", "PP", "from vitality"),
    ];

    public static LifeMatterQuestion Get(string referenceCode) =>
        All.FirstOrDefault(q => q.ReferenceCode == referenceCode) ?? All[0];

    private static readonly IReadOnlyDictionary<string, (string Short, string Theme)> Areas =
        new Dictionary<string, (string, string)>
        {
            ["SELF_HEALTH"] = ("Self & health", "the self"),
            ["SELF_IDENTITY"] = ("Identity", "my identity"),
            ["WEALTH"] = ("Wealth", "wealth"),
            ["CAREER_STATUS"] = ("Career", "career"),
            ["EDUCATION"] = ("Education", "education"),
            ["MARRIAGE_SPOUSE"] = ("Marriage", "marriage"),
            ["CHILDREN"] = ("Children", "children"),
            ["FAMILY_RELATIONSHIPS"] = ("Family", "family"),
            ["PROPERTY_COMFORTS"] = ("Property", "property and comforts"),
            ["SPIRITUALITY"] = ("Spirituality", "spirituality"),
            ["TROUBLE_LOSS"] = ("Trouble & loss", "trouble and loss"),
        };

    /// <summary>Short pill label; an unknown category falls back to its own name.</summary>
    public static string AreaLabel(string categoryCode, string categoryName) =>
        Areas.TryGetValue(categoryCode, out var a) ? a.Short : categoryName;

    public static string AreaTheme(string categoryCode, string categoryName) =>
        Areas.TryGetValue(categoryCode, out var a) ? a.Theme : categoryName.ToLowerInvariant();
}
