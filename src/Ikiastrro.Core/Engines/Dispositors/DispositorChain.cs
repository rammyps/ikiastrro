namespace Ikiastrro.Core.Engines.Dispositors;

public sealed record DispositorChain(
    string Planet,
    IReadOnlyList<string> Chain,
    string? FinalDispositor,
    bool InMutualReception,
    string TerminationCode = "UNKNOWN",
    IReadOnlyList<string>? Cycle = null);
