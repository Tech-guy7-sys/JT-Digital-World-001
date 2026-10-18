using System.IO;
using System.Diagnostics;

namespace AgentJeff;

public enum ClamAvScanStatus { Clean, ThreatFound, EngineError }

public sealed record ClamAvScanResult(
    ClamAvScanStatus Status,
    int ExitCode,
    string TargetPath,
    string Output,
    TimeSpan Duration);

/// <summary>
/// Runs the separately installed ClamAV command-line scanner. This does not install,
/// update, or silently modify ClamAV; the user must install it and maintain its database.
/// Exit codes follow ClamAV: 0 clean, 1 detections, 2 scan/engine error.
/// </summary>
public sealed class ClamAvScanner
{
    private static readonly TimeSpan ScanTimeout = TimeSpan.FromHours(2);

    public static string? FindClamScan()
    {
        var candidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ClamAV", "clamscan.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "ClamAV", "clamscan.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "ClamAV", "clamscan.exe")
        };

        foreach (var candidate in candidates)
            if (File.Exists(candidate)) return candidate;

        var pathValue = Environment.GetEnvironmentVariable("PATH") ?? "";
        foreach (var directory in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                var candidate = Path.Combine(directory.Trim('"'), "clamscan.exe");
                if (File.Exists(candidate)) return candidate;
            }
            catch { /* Ignore malformed PATH entries. */ }
        }
        return null;
    }

    public async Task<ClamAvScanResult> ScanFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var target = Path.GetFullPath(filePath);
        if (!File.Exists(target)) throw new FileNotFoundException("Selected file was not found.", target);
        return await ScanAsync(target, recursive: false, cancellationToken);
    }

    public async Task<ClamAvScanResult> ScanFolderAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        var target = Path.GetFullPath(folderPath);
        if (!Directory.Exists(target)) throw new DirectoryNotFoundException(target);
        return await ScanAsync(target, recursive: true, cancellationToken);
    }

    private static async Task<ClamAvScanResult> ScanAsync(string target, bool recursive, CancellationToken cancellationToken)
    {
        var scanner = FindClamScan()
            ?? throw new FileNotFoundException("ClamAV clamscan.exe was not found. Install ClamAV from the official download page, then configure/update its signature database. No file was scanned.");

        var psi = new ProcessStartInfo(scanner)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        psi.ArgumentList.Add("--infected");
        psi.ArgumentList.Add("--stdout");
        psi.ArgumentList.Add("--no-summary");
        if (recursive) psi.ArgumentList.Add("--recursive");
        psi.ArgumentList.Add(target);

        var timer = Stopwatch.StartNew();
        using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        if (!process.Start()) throw new InvalidOperationException("ClamAV scanner could not be started.");

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(ScanTimeout);
        using var registration = timeout.Token.Register(() =>
        {
            try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch { }
        });

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            try { await process.WaitForExitAsync(); } catch { }
            cancellationToken.ThrowIfCancellationRequested();
            throw new TimeoutException("ClamAV scan exceeded the two-hour safety limit and was stopped.");
        }

        var output = ((await stdoutTask) + Environment.NewLine + (await stderrTask)).Trim();
        timer.Stop();
        var status = MapExitCode(process.ExitCode);
        return new ClamAvScanResult(status, process.ExitCode, target, output, timer.Elapsed);
    }

    public static ClamAvScanStatus MapExitCode(int exitCode) => exitCode switch
    {
        0 => ClamAvScanStatus.Clean,
        1 => ClamAvScanStatus.ThreatFound,
        _ => ClamAvScanStatus.EngineError
    };
}
