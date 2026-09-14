using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Ikiastrro.Data;

public record JkdImportResult(int Added, int Skipped, string RawOutput);
public record JkdExportResult(int Exported, string RawOutput);

/// <summary>
/// Shells out to <c>tools/jkd-interchange/jkd-interchange.ps1</c> — Horoscope Explorer's .JKD
/// format needs the Jet 4.0 OLEDB provider, which only exists in-process at 32-bit on this
/// machine, so the script itself re-launches under the 32-bit PowerShell host when needed
/// (see the script's own top few lines). This service just runs it and parses its one-line
/// summary; it never touches SQL directly for this path — the script opens its own connection
/// (Windows Auth, same instance <see cref="SqlConnectionFactory"/> defaults to) and writes
/// straight to <c>tbl_BirthDetails</c>, independent of the repository layer.
///
/// Deliberately no <c>-SqlServer</c>/<c>-Database</c> override: this whole feature only runs on
/// this one dev machine (Jet 4.0 is a 32-bit-only, Windows-only provider), so there is no
/// multi-environment drift to guard against — the script's own default already matches
/// <see cref="SqlConnectionFactory"/>'s (db/README.md).
/// </summary>
public class JkdInterchangeService
{
    public async Task<JkdImportResult> ImportAsync(string jkdFilePath, CancellationToken ct = default)
    {
        var output = await RunAsync("import", jkdFilePath, force: false, ct);
        // "Imported 3 record(s); skipped 1 existing name(s)."
        return new JkdImportResult(ParseInt(output, @"Imported (\d+)"), ParseInt(output, @"skipped (\d+)"), output);
    }

    public async Task<JkdExportResult> ExportAsync(string jkdFilePath, CancellationToken ct = default)
    {
        // Always -Force: the caller always exports to a fresh temp path (see the /export/people.jkd
        // endpoint), so "target already exists" can only mean a stale temp file, safe to replace.
        var output = await RunAsync("export", jkdFilePath, force: true, ct);
        // "Exported 8 record(s) to C:\...\export.JKD"
        return new JkdExportResult(ParseInt(output, @"Exported (\d+)"), output);
    }

    private static async Task<string> RunAsync(string command, string path, bool force, CancellationToken ct)
    {
        var scriptPath = Path.Combine(FindRepoRoot(), "tools", "jkd-interchange", "jkd-interchange.ps1");
        var psi = new ProcessStartInfo("powershell.exe")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var arg in new[] { "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", scriptPath, command, path })
            psi.ArgumentList.Add(arg);
        if (force) psi.ArgumentList.Add("-Force");

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Could not start powershell.exe for jkd-interchange.ps1.");
        var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);
        var stdout = (await stdoutTask).Trim();
        var stderr = (await stderrTask).Trim();

        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"jkd-interchange.ps1 {command} failed (exit {process.ExitCode}): {(stderr.Length > 0 ? stderr : stdout)}");
        return stdout;
    }

    private static int ParseInt(string text, string pattern)
    {
        var m = Regex.Match(text, pattern);
        return m.Success ? int.Parse(m.Groups[1].Value) : 0;
    }

    /// <summary>Walks up from the app base dir to the folder holding <c>Ikiastrro.slnx</c>. Same
    /// approach as <c>Ikiastrro.Cli/TerminologySeed.cs.FindRepoRoot</c> — duplicated rather than
    /// shared because Cli depends on Data, not the other way around.</summary>
    private static string FindRepoRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "Ikiastrro.slnx")))
                return dir.FullName;
        throw new InvalidOperationException("Could not locate repo root (Ikiastrro.slnx) from " + AppContext.BaseDirectory);
    }
}
