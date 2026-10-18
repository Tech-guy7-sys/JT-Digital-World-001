using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AgentJeff;

public enum FileRisk { Clean, Review, Suspicious, Error }

public sealed record FileScanResult(string Path, string Sha256, FileRisk Risk, string Summary);

/// <summary>
/// First-pass local file triage for Agent Jeff. This is not a malware signature engine:
/// it hashes files and flags a few transparent heuristics, then can request a Microsoft
/// Defender custom scan. It never deletes, quarantines, or executes scanned content.
/// </summary>
public sealed class AntivirusEngine
{
    private static readonly string[] ScriptExtensions = [".ps1", ".psm1", ".bat", ".cmd", ".vbs", ".js", ".jse", ".hta", ".wsf"];
    private static readonly string[] ExecutableExtensions = [".exe", ".dll", ".scr", ".com", ".msi", ".sys"];
    private static readonly Regex[] SuspiciousScriptPatterns =
    [
        new(@"\b(FromBase64String|EncodedCommand)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\b(Invoke-Expression|IEX)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\b(DownloadString|DownloadFile)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\b(Set-MpPreference|Add-MpPreference)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\b(Startup|CurrentVersion\\Run)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)
    ];

    public async Task<IReadOnlyList<FileScanResult>> ScanFolderAsync(
        string folder, IProgress<(int completed, int total, string current)>? progress,
        CancellationToken cancellationToken)
    {
        var root = Path.GetFullPath(folder);
        if (!Directory.Exists(root)) throw new DirectoryNotFoundException(root);

        // Bound resource use and avoid following reparse points into unrelated trees.
        // Stream paths instead of materializing the whole tree. The previous approach
        // could traverse and allocate an arbitrarily large directory tree before applying
        // the 20,000-file cap. The total is intentionally reported as unknown (0) while
        // scanning; a hard cap takes priority over an expensive pre-count pass.
        var results = new List<FileScanResult>();
        foreach (var file in EnumerateFilesSafe(root, cancellationToken).Take(20_000))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var index = results.Count + 1;
            progress?.Report((index, 0, file));
            try
            {
                results.Add(await Task.Run(() => InspectFile(file, cancellationToken), cancellationToken));
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { results.Add(new FileScanResult(file, "", FileRisk.Error, ex.Message)); }
        }
        return results;
    }

    public async Task<FileScanResult> ScanFileAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("Selected file was not found.", fullPath);
        return await Task.Run(() => InspectFile(fullPath, cancellationToken), cancellationToken);
    }

    private static IEnumerable<string> EnumerateFilesSafe(string root, CancellationToken token)
    {
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            token.ThrowIfCancellationRequested();
            var current = pending.Pop();
            IEnumerable<DirectoryInfo> dirs;
            IEnumerable<string> files;
            try
            {
                var info = new DirectoryInfo(current);
                if ((info.Attributes & FileAttributes.ReparsePoint) != 0) continue;
                // Keep enumeration lazy so Take(20_000) can stop traversal promptly.
                dirs = info.EnumerateDirectories();
                files = info.EnumerateFiles()
                    .Where(f => (f.Attributes & FileAttributes.ReparsePoint) == 0)
                    .Select(f => f.FullName);
            }
            catch { continue; }
            foreach (var file in files) yield return file;
            foreach (var dir in dirs)
            {
                try { if ((dir.Attributes & FileAttributes.ReparsePoint) == 0) pending.Push(dir.FullName); }
                catch { /* inaccessible directory */ }
            }
        }
    }

    private static FileScanResult InspectFile(string path, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var sha = SHA256.Create();
        var hash = Convert.ToHexString(sha.ComputeHash(stream));
        var ext = Path.GetExtension(path).ToLowerInvariant();
        var findings = new List<string>();
        var risk = FileRisk.Clean;

        // Exact-content test signature; EICAR is a harmless antivirus test file.
        // Keep file reads bounded to avoid loading large untrusted files into memory.
        var signatureDb = new ThreatSignatureDatabase();
        if (new FileInfo(path).Length <= 2_000_000)
        {
            try
            {
                var bytes = File.ReadAllBytes(path);
                var signatureMatch = signatureDb.MatchHash(hash) ?? signatureDb.MatchContent(bytes);
                if (signatureMatch is not null)
                {
                    findings.Add("Signature match: " + signatureMatch);
                    risk = FileRisk.Suspicious;
                }
            }
            catch (IOException) { /* The file may have changed since hashing. */ }
            catch (UnauthorizedAccessException) { /* Continue with available metadata checks. */ }
        }

        var name = Path.GetFileName(path);
        var parts = name.Split('.');
        if (parts.Length >= 3 && ExecutableExtensions.Contains("." + parts[^1], StringComparer.OrdinalIgnoreCase)
            && parts.Take(parts.Length - 1).Any(p => p.Equals("pdf", StringComparison.OrdinalIgnoreCase)
                || p.Equals("doc", StringComparison.OrdinalIgnoreCase) || p.Equals("docx", StringComparison.OrdinalIgnoreCase)
                || p.Equals("jpg", StringComparison.OrdinalIgnoreCase) || p.Equals("png", StringComparison.OrdinalIgnoreCase)))
        {
            findings.Add("Double-extension executable name"); risk = FileRisk.Suspicious;
        }

        var temp = Path.GetTempPath();
        var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        if (ExecutableExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase)
            && (path.StartsWith(temp, StringComparison.OrdinalIgnoreCase) || path.StartsWith(downloads, StringComparison.OrdinalIgnoreCase)))
        {
            findings.Add("Executable located in Temp or Downloads; verify its source");
            if (risk == FileRisk.Clean) risk = FileRisk.Review;
        }

        var info = new FileInfo(path);
        if (info.Length <= 2_000_000 && ScriptExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
        {
            token.ThrowIfCancellationRequested();
            string text;
            try { text = File.ReadAllText(path, Encoding.UTF8); }
            catch { text = ""; }
            foreach (var pattern in SuspiciousScriptPatterns)
            {
                if (!pattern.IsMatch(text)) continue;
                findings.Add("Script contains potentially risky pattern: " + pattern.ToString().Split('\\')[2].TrimEnd(')'));
                risk = FileRisk.Suspicious;
            }
        }

        return new FileScanResult(path, hash, risk,
            findings.Count == 0 ? "No heuristic flags. This does not prove the file is safe." : string.Join("; ", findings));
    }

    public static async Task<(int ExitCode, string Output)> RequestDefenderCustomScanAsync(
        string targetPath, CancellationToken cancellationToken = default)
    {
        var scanner = FindMpCmdRun();
        if (scanner is null) throw new FileNotFoundException("Microsoft Defender scanner was not found.");
        var psi = new ProcessStartInfo(scanner)
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        psi.ArgumentList.Add("-Scan");
        psi.ArgumentList.Add("-ScanType");
        psi.ArgumentList.Add("3");
        psi.ArgumentList.Add("-File");
        psi.ArgumentList.Add(Path.GetFullPath(targetPath));
        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Could not start Defender scanner.");
        using var registration = cancellationToken.Register(() => { try { process.Kill(true); } catch { } });
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(cancellationToken);
        return (process.ExitCode, (await stdout) + Environment.NewLine + (await stderr));
    }

    private static string? FindMpCmdRun()
    {
        var roots = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows Defender"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Microsoft", "Windows Defender", "Platform")
        };
        foreach (var root in roots)
        {
            if (!Directory.Exists(root)) continue;
            try
            {
                var files = Directory.GetFiles(root, "MpCmdRun.exe", SearchOption.AllDirectories);
                if (files.Length > 0) return files.OrderByDescending(File.GetLastWriteTimeUtc).First();
            }
            catch { /* Defender platform folders may be protected */ }
        }
        return null;
    }
}
