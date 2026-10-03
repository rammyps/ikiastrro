using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Matching;

/// <summary>Where a malefic is counted from: the Ascendant, the Moon, or Venus (the Kalatrakaraka).</summary>
public enum DoshaReference { Lagna, Moon, Venus }

/// <summary>How the source weighs an affliction (Vasudev, Ch. V, p.54 and p.60). Untiered: listed as an
/// affliction but given no weight. Conditional: Mars or Saturn in the 1st, which the book says depends
/// on the sign involved.</summary>
public enum DoshaWeight { Severe, Notable, Lesser, Conditional, Untiered }

/// <summary>One malefic in an afflicted house counted from one reference point.</summary>
public sealed record DoshaAffliction(PlanetName Malefic, DoshaReference From, int House, DoshaWeight Weight, string Note);

/// <summary>The positions Dosha Samya reads: the Ascendant sign and the D1 sign of each planet.</summary>
public sealed record DoshaChart(ZodiacName Lagna, IReadOnlyDictionary<PlanetName, ZodiacName> Signs);

/// <summary>A person's Dosha Samya reading. <see cref="MarsHouses"/> is Mars counted from each reference
/// (the "Mars in the 1st, 2nd, 4th, 7th, 8th, 12th" check), whatever the book weighs.</summary>
public sealed record DoshaReading(
    IReadOnlyList<DoshaAffliction> Afflictions,
    IReadOnlyDictionary<DoshaReference, int> MarsHouses)
{
    public IEnumerable<DoshaAffliction> Severe => Afflictions.Where(a => a.Weight == DoshaWeight.Severe);

    /// <summary>Mars in the 1st, 2nd, 4th, 7th, 8th or 12th from any reference: the classical Manglik test.</summary>
    public bool ClassicalKuja => MarsHouses.Values.Any(h => h is 1 or 2 or 4 or 7 or 8 or 12);
}

public enum DoshaBalanceStatus { NoSevereDosha, Balanced, PartlyBalanced, NotBalanced }

public sealed record DoshaBalance(DoshaBalanceStatus Status, string Reason, string SourceLocator);

/// <summary>
/// Dosha Samya ("balancing of malefic content") as Vasudev, "The Art of Matching Charts", Ch. V states
/// it (pp.51-61; <c>SRC_VASUDEV_MATCHING_CHARTS</c>):
///  - Sun, Mars, Saturn, Rahu and Ketu in the 2nd, 4th, 7th, 8th or 12th (and Mars or Saturn in the 1st)
///    from the Lagna, the Moon or Venus afflict marriage (p.53-54, p.60).
///  - Mars or Rahu in the 7th or 8th are the more dangerous; Rahu in the 4th is to be handled
///    cautiously; Mars in the 2nd, 4th or 12th, and Rahu in the 2nd or 12th, are less malevolent (p.54).
///  - Mars in the 1st is highly inimical in Taurus, Gemini, Virgo or Scorpio (p.55).
///  - A chart with Mars in the 7th or 8th needs a similar placement of Mars in the other; for a girl with
///    Mars in the 8th the boy too should have it in the 8th, but for Mars in the 7th the boy's 7th or 8th
///    is fine; Mars in the 2nd, 4th or 12th in one chart cannot offset the 7th or 8th in the other
///    (pp.56-58).
///
/// Where the book is silent this class does not decide: Sun, Saturn and Ketu are listed without a
/// weight, and the book states the balancing rule for Mars only. Reading Rahu in the 7th or 8th under the
/// same rule (the book names Mars and Rahu together as the more dangerous) is this project's extension and
/// is said so in the result. The book also says these results are not absolute (p.54). Pure; no I/O.
/// </summary>
public static class DoshaSamyaCalculator
{
    private static readonly PlanetName[] Malefics =
        [PlanetName.Sun, PlanetName.Mars, PlanetName.Saturn, PlanetName.Rahu, PlanetName.Ketu];

    public static DoshaReading Read(DoshaChart chart)
    {
        var references = new (DoshaReference Ref, ZodiacName Sign)[]
        {
            (DoshaReference.Lagna, chart.Lagna),
            (DoshaReference.Moon, chart.Signs[PlanetName.Moon]),
            (DoshaReference.Venus, chart.Signs[PlanetName.Venus]),
        };

        var afflictions = new List<DoshaAffliction>();
        var marsHouses = new Dictionary<DoshaReference, int>();

        foreach (var (reference, refSign) in references)
        {
            marsHouses[reference] = HouseFrom(refSign, chart.Signs[PlanetName.Mars]);
            foreach (var malefic in Malefics)
            {
                var house = HouseFrom(refSign, chart.Signs[malefic]);
                var (weight, note) = Weigh(malefic, house, reference, chart.Signs[malefic]);
                if (note is not null)
                    afflictions.Add(new DoshaAffliction(malefic, reference, house, weight, note));
            }
        }
        return new DoshaReading(afflictions, marsHouses);
    }

    /// <summary>Position of <paramref name="sign"/> counted from <paramref name="from"/> (same sign = 1).</summary>
    internal static int HouseFrom(ZodiacName from, ZodiacName sign) => (((int)sign - (int)from + 12) % 12) + 1;

    // Returns a null note when the placement is not an affliction the book names.
    private static (DoshaWeight, string?) Weigh(PlanetName malefic, int house, DoshaReference from, ZodiacName malefSign)
    {
        if (house == 1)
        {
            if (malefic == PlanetName.Mars)
            {
                var inimical = from != DoshaReference.Venus && malefSign is ZodiacName.Taurus or ZodiacName.Gemini or ZodiacName.Virgo or ZodiacName.Scorpio;
                return (DoshaWeight.Conditional, inimical
                    ? "Mars in the 1st in Taurus, Gemini, Virgo or Scorpio: highly inimical to marital happiness (p.55)."
                    : "Mars in the 1st: can be disastrous depending on the Lagna and Chandra Lagna involved (p.54).");
            }
            return malefic == PlanetName.Saturn
                ? (DoshaWeight.Conditional, "Saturn in the 1st: can be disastrous depending on the Lagna and Chandra Lagna involved (p.54).")
                : (DoshaWeight.Untiered, null);
        }

        if (house is not (2 or 4 or 7 or 8 or 12)) return (DoshaWeight.Untiered, null);

        return (malefic, house) switch
        {
            (PlanetName.Mars or PlanetName.Rahu, 7 or 8) => (DoshaWeight.Severe, "Mars or Rahu in the 7th or 8th: the more dangerous (p.54)."),
            (PlanetName.Rahu, 4) => (DoshaWeight.Notable, "Rahu in the 4th: to be handled cautiously (p.54)."),
            (PlanetName.Mars, 2 or 4 or 12) => (DoshaWeight.Lesser, "Mars in the 2nd, 4th or 12th: relatively less malevolent (p.54)."),
            (PlanetName.Rahu, 2 or 12) => (DoshaWeight.Lesser, "Rahu in the 2nd or 12th: relatively less malevolent (p.54)."),
            _ => (DoshaWeight.Untiered, "Listed as an affliction without a stated weight (p.53, p.60)."),
        };
    }

    /// <summary>The book's balancing test between the boy's and the girl's readings.</summary>
    public static DoshaBalance Balance(DoshaReading boy, DoshaReading girl)
    {
        var boySevere = boy.Severe.ToList();
        var girlSevere = girl.Severe.ToList();

        if (boySevere.Count == 0 && girlSevere.Count == 0)
            return new(DoshaBalanceStatus.NoSevereDosha,
                "Neither chart has Mars or Rahu in the 7th or 8th from the Lagna, Moon or Venus.", "p.54, p.56-58");

        if (boySevere.Count == 0 || girlSevere.Count == 0)
        {
            var who = boySevere.Count > 0 ? "The boy's" : "The girl's";
            return new(DoshaBalanceStatus.NotBalanced,
                $"{who} chart has Mars or Rahu in the 7th or 8th and the other chart has none. The book says Mars in the 2nd, 4th or 12th in the other chart cannot keep that at bay; look for a similar placement in both.",
                "p.56-58");
        }

        var girlMarsEighthOnly = girlSevere.All(a => a.House == 8);
        var boyHasEighth = boySevere.Any(a => a.House == 8);
        if (girlMarsEighthOnly && !boyHasEighth)
            return new(DoshaBalanceStatus.PartlyBalanced,
                "Both charts carry Mars or Rahu in the 7th or 8th, but the girl's is in the 8th only and the boy's is in the 7th; the book prefers the boy's also in the 8th.",
                "p.57");

        return new(DoshaBalanceStatus.Balanced,
            "Both charts carry a similar measure of Mars or Rahu in the 7th or 8th, which the book counts as balanced. Reading Rahu under the rule the book states for Mars is this project's extension.",
            "p.56-58");
    }
}
