namespace Ikiastrro.Core.Numerology;

/// <summary>
/// Cheiro's name-number method (the Chaldean letter-value table Cheiro published and popularised)
/// — ported from the ikinumero prototype (D:\@ChatGPT\ikinumero) into ikiastrro's own stack. The
/// letter-value table is a fixed classical standard (9 is sacred/unassigned) that no user ever
/// edits, so it is hardcoded here rather than stored as DB reference data — a migration/repo
/// would only add indirection.
/// </summary>
public static class CheiroNumerology
{
    private static readonly IReadOnlyDictionary<char, int> LetterValues = BuildLetterValues();

    private static IReadOnlyDictionary<char, int> BuildLetterValues()
    {
        ReadOnlySpan<(int Value, string Letters)> groups =
        [
            (1, "AIJQY"),
            (2, "BKR"),
            (3, "CGLS"),
            (4, "DMT"),
            (5, "EHNX"),
            (6, "UVW"),
            (7, "OZ"),
            (8, "FP"),
        ];

        var map = new Dictionary<char, int>();
        foreach (var (value, letters) in groups)
            foreach (var letter in letters)
                map[letter] = value;
        return map;
    }

    public static NumerologyResult Calculate(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var letters = name.ToUpperInvariant()
            .Where(c => c is >= 'A' and <= 'Z')
            .Select(c => new NumerologyLetterValue(c, LetterValues.GetValueOrDefault(c)))
            .Where(l => l.Value > 0)
            .ToList();

        var compoundTotal = letters.Sum(l => l.Value);
        var rootNumber = ReduceToSingleDigit(compoundTotal);
        return new NumerologyResult(name.Trim(), compoundTotal, rootNumber, letters);
    }

    private static int ReduceToSingleDigit(int number)
    {
        while (number > 9) number = number.ToString().Sum(c => c - '0');
        return number;
    }
}

public readonly record struct NumerologyLetterValue(char Letter, int Value);

public sealed record NumerologyResult(
    string Name,
    int CompoundTotal,
    int RootNumber,
    IReadOnlyList<NumerologyLetterValue> Letters);
