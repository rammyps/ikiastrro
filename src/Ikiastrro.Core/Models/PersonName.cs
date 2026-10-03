namespace Ikiastrro.Core.Models;

/// <summary>Helpers for the first-name / last-name entry fields on top of the single stored Name.</summary>
public static class PersonName
{
    /// <summary>The stored display name: first and last joined by one space, trimmed.</summary>
    public static string Compose(string? first, string? last) =>
        string.Join(' ', new[] { first?.Trim(), last?.Trim() }.Where(p => !string.IsNullOrEmpty(p)));

    /// <summary>Splits free text typed into the search box into a first and last name: the last word is the
    /// last name, everything before it the first name. One word is a first name only.</summary>
    public static (string First, string Last) Split(string? text)
    {
        var words = (text ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return words.Length switch
        {
            0 => (string.Empty, string.Empty),
            1 => (words[0], string.Empty),
            _ => (string.Join(' ', words[..^1]), words[^1]),
        };
    }

    /// <summary>True when <paramref name="query"/> appears in <paramref name="name"/>, ignoring case and
    /// spaces, so "ananya r" finds a person saved as "AnanyaR" and "Ananya R" finds "Ananya R".</summary>
    public static bool Matches(string name, string query)
    {
        static string Squash(string s) => new(s.Where(c => !char.IsWhiteSpace(c)).ToArray());
        var q = Squash(query);
        return q.Length > 0 && Squash(name).Contains(q, StringComparison.OrdinalIgnoreCase);
    }
}
