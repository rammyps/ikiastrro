namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>A special point's D1 sidereal longitude, before any varga projection.
/// PointKind is 'Arudha' | 'GrahaArudha' | 'SpecialLagna' | 'Upagraha'. Code: 'AL', 'A2'..'A12',
/// 'GA_Sun'..'GA_Ketu', 'BL', 'HL', 'GL', 'SL', 'Gulika', 'Maandi'.</summary>
public sealed record SpecialPointSeed(string Code, string PointKind, double NirayanaLongitudeDegrees);
