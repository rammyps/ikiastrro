using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.DivisionalCharts;
using Ikiastrro.Core.Engines.Houses;

namespace Ikiastrro.Core.Engines.KeyInfo;

/// <summary>The five D3 schemes JHora offers, for the 22nd drekkana.</summary>
public enum DrekkanaScheme { Parasara, ParivrittiTraya, Somanatha, UmaShambhu, Jagannatha }

/// <summary>
/// Lords of the 64th navamsa and the 22nd drekkana — the "malicious divisions" of the longevity chapters —
/// counted from the position's own navamsa or drekkana. The 64th navamsa is the one 63 navamsas on; navamsa signs
/// run one sign per navamsa around the zodiac, so that is the navamsa sign + 3. The 22nd drekkana is the one 21
/// drekkanas on in the zodiac's 36-drekkana sequence <b>of the chosen scheme</b> — each scheme gives its own
/// sign to each of the 36 positions, so the answer differs by scheme. Matches JHora's "Lords of 64th navamsa and
/// 22nd drekkana" for 1_Ramakrishnan, from the Lagna and from the Moon, all 12 rows. Scheme tables: PyJHora
/// <c>parivritti_cyclic</c>, <c>parivritti_alternate</c>, <c>drekkana_jagannatha</c> (AGPL — tables only);
/// Uma-Shambhu is this repo's own <see cref="DrekkanaD3UmaShambuSignRule"/>.
/// </summary>
public static class MaliciousDivisions
{
    private static readonly int[][] Jagannatha =
        { new[] { 0, 4, 8 }, new[] { 9, 1, 5 }, new[] { 6, 10, 2 }, new[] { 3, 7, 11 } };

    private static PlanetName LordOfSign(int sign) =>
        Enum.Parse<PlanetName>(HouseEngine.GetSignLord((ZodiacName)(((sign % 12) + 12) % 12)));

    public static PlanetName Lord64thNavamsa(double longitude) =>
        LordOfSign((int)new NavamsaD9SignRule().SignFor(longitude) + 3);

    /// <summary>Sign a scheme gives the drekkana at zodiac position <paramref name="position"/> (0–35).</summary>
    public static int DrekkanaSign(DrekkanaScheme scheme, int position)
    {
        var r = position / 3;
        var l = position % 3;
        return scheme switch
        {
            DrekkanaScheme.Parasara => (r + 4 * l) % 12,
            DrekkanaScheme.ParivrittiTraya => (3 * r + l) % 12,
            DrekkanaScheme.Somanatha => r % 2 == 0
                ? (3 * (r / 2) + l) % 12
                : ((11 - 3 * (r / 2) - l) % 12 + 12) % 12,
            DrekkanaScheme.UmaShambhu => (int)new DrekkanaD3UmaShambuSignRule().SignFor(r * 30 + l * 10 + 1),
            _ => Jagannatha[r % 4][l],
        };
    }

    public static PlanetName Lord22ndDrekkana(DrekkanaScheme scheme, double longitude)
    {
        var position = (int)(AstroMath.Normalize(longitude) / 10);
        return LordOfSign(DrekkanaSign(scheme, (position + 21) % 36));
    }
}
