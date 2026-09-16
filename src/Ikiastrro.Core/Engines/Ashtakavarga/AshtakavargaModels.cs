namespace Ikiastrro.Core.Engines.Ashtakavarga;

/// <summary>One recipient graha's Bhinnāṣṭakavarga — 12 bindu counts, Aries…Pisces (0-based).</summary>
public sealed record BhinnaAshtakavarga(string Recipient, IReadOnlyList<int> Bindus, int Total);

/// <summary>
/// The 1/0 explain layer: whether one contributor earned the recipient a bindu in one sign.
/// <see cref="SignNumber"/> and <see cref="ContributorSignNumber"/> are 1-based (1 = Aries).
/// </summary>
public sealed record AshtakavargaContributionCell(
    string Recipient,
    string Contributor,
    int SignNumber,
    bool IsBindu,
    int ContributorSignNumber);

/// <summary>Sarvāṣṭakavarga — the per-sign total across the seven planetary BAVs (Lagna excluded).</summary>
public sealed record SarvaAshtakavarga(IReadOnlyList<int> Bindus)
{
    public int Total => Bindus.Sum();
}

/// <summary>Piṇḍa reductions for one recipient, computed on the post-Śodhana BAV.</summary>
public sealed record AshtakavargaPinda(string Recipient, int RasiPinda, int GrahaPinda, int SodhyaPinda);

/// <summary>
/// The full Parāśari Ashtakavarga for one D1 chart: raw and post-Śodhana Bhinnāṣṭakavargas,
/// the Sarva total, the contribution detail, and the Piṇḍa reductions.
/// </summary>
public sealed record AshtakavargaResult(
    IReadOnlyList<BhinnaAshtakavarga> Bhinna,
    IReadOnlyList<BhinnaAshtakavarga> Reduced,
    SarvaAshtakavarga Sarva,
    IReadOnlyList<AshtakavargaContributionCell> Contributions,
    IReadOnlyList<AshtakavargaPinda> Pinda)
{
    public const string MethodCode = "PVR_PARASARA_BAV";
    public const string SourceRefCode = "SRC_BPHS_ASHTAKAVARGA";
}
