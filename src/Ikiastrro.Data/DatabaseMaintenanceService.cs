using System.Diagnostics;

namespace Ikiastrro.Data;

/// <summary>
/// Shells out to the two dev-only PowerShell launchers under db/checks — <c>Reset-AllTransactionalData.ps1</c>
/// (full wipe: people, chart results, analytics, facts) and <c>Rebuild-AllGeneratedData.ps1</c>
/// (force-recalculates every saved person's charts + Dasha via <c>dotnet run -- rebuild-all</c>).
/// Backs the "DB RESET" and "RECALCULATE" buttons on SavedCharts.razor. Same shape as
/// <see cref="JkdInterchangeService"/>: no <c>-Server</c>/<c>-Database</c> override — this only
/// runs on this one dev machine, and both scripts' own defaults already match
/// <see cref="SqlConnectionFactory"/>'s.
/// </summary>
public class DatabaseMaintenanceService
{
    public Task<string> ResetAllDataAsync(CancellationToken ct = default) =>
        RunAsync(Path.Combine("db", "checks", "Reset-AllTransactionalData.ps1"), ct);

    public Task<string> RecalculateAllChartsAsync(CancellationToken ct = default) =>
        RunAsync(Path.Combine("db", "checks", "Rebuild-AllGeneratedData.ps1"), ct);

    private static async Task<string> RunAsync(string relativeScriptPath, CancellationToken ct)
    {
        var scriptPath = Path.Combine(FindRepoRoot(), relativeScriptPath);
        var psi = new ProcessStartInfo("powershell.exe")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var arg in new[] { "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", scriptPath, "-Confirm:$false" })
            psi.ArgumentList.Add(arg);

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException($"Could not start powershell.exe for {relativeScriptPath}.");
        var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);
        var stdout = (await stdoutTask).Trim();
        var stderr = (await stderrTask).Trim();

        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"{relativeScriptPath} failed (exit {process.ExitCode}): {(stderr.Length > 0 ? stderr : stdout)}");
        return stdout;
    }

    /// <summary>Walks up from the app base dir to the folder holding <c>Ikiastrro.slnx</c>. Same
    /// approach as <see cref="JkdInterchangeService"/>'s FindRepoRoot — duplicated rather than
    /// shared per that class's own note on why (Cli depends on Data, not the other way around;
    /// this one lives in Data itself, but keeping the same small helper avoids a cross-cutting
    /// utility for two callers).</summary>
    private static string FindRepoRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "Ikiastrro.slnx")))
                return dir.FullName;
        throw new InvalidOperationException("Could not locate repo root (Ikiastrro.slnx) from " + AppContext.BaseDirectory);
    }
}
