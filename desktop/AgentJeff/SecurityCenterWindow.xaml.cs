using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace AgentJeff;

public partial class SecurityCenterWindow : Window
{
    public SecurityCenterWindow() => InitializeComponent();

    private static ProcessStartInfo Hidden(string file, string args) => new(file, args) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };

    private async void CheckStatus_Click(object sender, RoutedEventArgs e)
    {
        ProtectionStatus.Text = "Checking…";
        try
        {
            var psi = Hidden("powershell.exe", "-NoProfile -NonInteractive -Command \"$s=Get-MpComputerStatus; [pscustomobject]@{AMServiceEnabled=$s.AMServiceEnabled;AntivirusEnabled=$s.AntivirusEnabled;RealTimeProtectionEnabled=$s.RealTimeProtectionEnabled;AntivirusSignatureLastUpdated=$s.AntivirusSignatureLastUpdated;QuickScanAge=$s.QuickScanAge} | ConvertTo-Json -Compress\"");
            using var p = Process.Start(psi);
            if (p is null) throw new InvalidOperationException("Could not start the Windows status query.");
            var stdoutTask = p.StandardOutput.ReadToEndAsync(); var stderrTask = p.StandardError.ReadToEndAsync();
            await p.WaitForExitAsync();
            var output = await stdoutTask; var error = await stderrTask;
            if (p.ExitCode != 0 || string.IsNullOrWhiteSpace(output)) throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? "Defender status was unavailable. Windows Security may be managed by another antivirus." : error.Trim());
            using var doc = System.Text.Json.JsonDocument.Parse(output);
            var root = doc.RootElement;
            bool GetBool(string name) => root.TryGetProperty(name, out var v) && v.ValueKind == System.Text.Json.JsonValueKind.True;
            var enabled = GetBool("AntivirusEnabled");
            var realtime = GetBool("RealTimeProtectionEnabled");
            ProtectionStatus.Text = enabled && realtime ? "Antivirus and real-time protection ON" : "Protection needs attention";
            var updated = root.TryGetProperty("AntivirusSignatureLastUpdated", out var sig) ? sig.ToString() : "Unknown";
            var age = root.TryGetProperty("QuickScanAge", out var scan) ? scan.ToString() : "Unknown";
            ProtectionDetails.Text = $"Defender service: {(GetBool("AMServiceEnabled") ? "running" : "not running")} · Antivirus: {(enabled ? "enabled" : "disabled")} · Real-time: {(realtime ? "on" : "off")}\nSecurity intelligence last updated: {updated}\nQuick scan age (days): {age}";
        }
        catch (Exception ex) { ProtectionStatus.Text = "Could not verify Defender status"; ProtectionDetails.Text = ex.Message; }
    }

    private async void AnalyzeFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Choose a file to analyze", CheckFileExists = true, Multiselect = false, Filter = "All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) != true) return;
        EngineStatus.Text = "Analyzing selected file…";
        try
        {
            var engine = new AntivirusEngine();
            var result = await engine.ScanFileAsync(dialog.FileName);
            EngineStatus.Text = $"Local triage: {result.Risk}\nSHA-256: {result.Sha256}\n{result.Summary}";
            MessageBox.Show(this, $"File: {result.Path}\nTriage: {result.Risk}\nSHA-256: {result.Sha256}\n\n{result.Summary}\n\nHeuristic triage can miss malware and can flag harmless files. Use Defender for an antivirus scan.", "Jeff file analysis", MessageBoxButton.OK, result.Risk == FileRisk.Suspicious ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }
        catch (Exception ex) { EngineStatus.Text = "File analysis failed: " + ex.Message; }
    }

    private async void ScanFolder_Click(object sender, RoutedEventArgs e)
    {
        var folderDialog = new OpenFolderDialog { Title = "Choose a folder for local file triage" };
        if (folderDialog.ShowDialog(this) != true) return;
        var folder = folderDialog.FolderName;
        var approved = MessageBox.Show(this,
            $"Analyze files under this folder?\n\nFolder: {folder}\nLimit: 20,000 files. Reparse-point folders are skipped. This computes hashes and checks simple heuristics; it does not prove files are safe or replace Defender. No files will be changed.",
            "Approve folder triage", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (approved != MessageBoxResult.Yes) { EngineStatus.Text = "Folder scan denied."; return; }

        var saveDialog = new SaveFileDialog { Title = "Save local triage report", Filter = "CSV report (*.csv)|*.csv", FileName = "Jeff-File-Triage-Report.csv", AddExtension = true, DefaultExt = ".csv" };
        if (saveDialog.ShowDialog(this) != true) { EngineStatus.Text = "Report save cancelled; no folder scan started."; return; }

        EngineStatus.Text = "Scanning folder…";
        try
        {
            var progress = new Progress<(int completed, int total, string current)>(p =>
                EngineStatus.Text = p.total > 0
                    ? $"Scanning {p.completed}/{p.total}: {Path.GetFileName(p.current)}"
                    : $"Scanning file {p.completed} (maximum 20,000): {Path.GetFileName(p.current)}");
            var engine = new AntivirusEngine();
            var results = await engine.ScanFolderAsync(folder, progress, CancellationToken.None);
            static string Csv(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
            var lines = new List<string> { "Path,SHA256,Risk,Summary" };
            lines.AddRange(results.Select(x => string.Join(",", Csv(x.Path), Csv(x.Sha256), Csv(x.Risk.ToString()), Csv(x.Summary))));
            await File.WriteAllLinesAsync(saveDialog.FileName, lines, new System.Text.UTF8Encoding(true));
            var suspicious = results.Count(x => x.Risk == FileRisk.Suspicious);
            var review = results.Count(x => x.Risk == FileRisk.Review);
            var errors = results.Count(x => x.Risk == FileRisk.Error);
            EngineStatus.Text = $"Folder triage finished: {results.Count} files; {suspicious} suspicious heuristic flags; {review} review flags; {errors} errors. Report: {saveDialog.FileName}";
            MessageBox.Show(this, EngineStatus.Text + "\n\nThese are heuristic indicators only. Review flagged files and use Microsoft Defender for actual antivirus scanning.", "Folder triage report", MessageBoxButton.OK,
                suspicious > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }
        catch (Exception ex) { EngineStatus.Text = "Folder scan failed: " + ex.Message; }
    }

    private async void DefenderFileScan_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Choose a file for Microsoft Defender to scan", CheckFileExists = true, Multiselect = false, Filter = "All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) != true) return;
        var approved = MessageBox.Show(this,
            $"Request a Microsoft Defender custom scan?\n\nTarget: {dialog.FileName}\n\nDefender will inspect the selected file using its installed security intelligence. This does not guarantee detection. Jeff will not delete or quarantine the file.",
            "Approve Defender file scan", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (approved != MessageBoxResult.Yes) { EngineStatus.Text = "Defender file scan denied."; return; }
        EngineStatus.Text = "Requesting Defender custom scan…";
        try
        {
            var result = await AntivirusEngine.RequestDefenderCustomScanAsync(dialog.FileName);
            EngineStatus.Text = result.ExitCode == 0
                ? "Defender scan command completed. Review Windows Security for detections. " + result.Output.Trim()
                : $"Defender scan returned exit code {result.ExitCode}. {result.Output.Trim()}";
            MessageBox.Show(this, EngineStatus.Text, "Defender custom scan result", MessageBoxButton.OK,
                result.ExitCode == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
        catch (Exception ex) { EngineStatus.Text = "Defender scan failed: " + ex.Message; }
    }

    private async void QuickScan_Click(object sender, RoutedEventArgs e)
    {
        var answer = MessageBox.Show(this, "Jeff will request a Microsoft Defender quick scan. It can use CPU while it checks common threat locations. Jeff will not disable protection or delete files. Continue?", "Approve Defender quick scan", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes) { ScanStatus.Text = "Quick scan denied by user."; return; }
        try
        {
            var path = FindMpCmdRun();
            if (path is null) throw new FileNotFoundException("Microsoft Defender command-line scanner was not found. Check Windows Security.");
            ScanStatus.Text = "Starting Defender quick scan…";
            var psi = Hidden(path, "-Scan -ScanType 1");
            using var p = Process.Start(psi) ?? throw new InvalidOperationException("Could not start Defender scanner.");
            var outputTask = p.StandardOutput.ReadToEndAsync(); var errorTask = p.StandardError.ReadToEndAsync();
            await p.WaitForExitAsync();
            var output = await outputTask; var error = await errorTask;
            ScanStatus.Text = p.ExitCode == 0 ? "Defender quick scan command completed. Review Windows Security for findings." : $"Scan returned exit code {p.ExitCode}. {error} {output}".Trim();
            MessageBox.Show(this, ScanStatus.Text, "Defender scan result", MessageBoxButton.OK, p.ExitCode == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
        catch (Exception ex) { ScanStatus.Text = ex.Message; MessageBox.Show(this, ex.Message, "Scan unavailable", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    private async void ScheduleScan_Click(object sender, RoutedEventArgs e)
    {
        var message = "Create a Windows Task Scheduler task that requests a Microsoft Defender quick scan daily at 2:00 AM? Windows may reject this without administrator rights. You can remove the task later in Task Scheduler under JT Agent Jeff Daily Defender Quick Scan.";
        if (MessageBox.Show(this, message, "Approve daily scan schedule", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) { ScanStatus.Text = "Daily schedule was not created."; return; }
        try
        {
            var scanner = FindMpCmdRun() ?? throw new FileNotFoundException("Microsoft Defender scanner was not found.");
            // Use ArgumentList so the task name and scanner path are passed as single arguments.
            var psi = new ProcessStartInfo("schtasks.exe")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            foreach (var arg in new[]
            {
                "/Create", "/F", "/SC", "DAILY", "/ST", "02:00",
                "/TN", "JT Agent Jeff Daily Defender Quick Scan",
                "/TR", $"\\\"{scanner}\\\" -Scan -ScanType 1"
            }) psi.ArgumentList.Add(arg);
            using var p = Process.Start(psi) ?? throw new InvalidOperationException("Could not start Task Scheduler.");
            var stdoutTask = p.StandardOutput.ReadToEndAsync(); var stderrTask = p.StandardError.ReadToEndAsync();
            await p.WaitForExitAsync(); var output = await stdoutTask; var error = await stderrTask;
            ScanStatus.Text = p.ExitCode == 0 ? "Daily scan task created for 2:00 AM." : $"Task Scheduler could not create it (exit {p.ExitCode}). {error} {output}".Trim();
            MessageBox.Show(this, ScanStatus.Text, "Daily scan schedule", MessageBoxButton.OK, p.ExitCode == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
        catch (Exception ex) { ScanStatus.Text = ex.Message; MessageBox.Show(this, ex.Message, "Scheduling unavailable", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    private void Diagnostic_Click(object sender, RoutedEventArgs e)
    {
        if (DiagnosticPicker.SelectedItem is not ComboBoxItem item || item.Tag is not string tag) return;
        string file, args, description, risk;
        switch (tag)
        {
            case "ipconfig": file = "ipconfig.exe"; args = "/all"; description = "Display network adapter configuration"; risk = "Low · read-only network information."; break;
            case "dism": file = "dism.exe"; args = "/Online /Cleanup-Image /CheckHealth"; description = "Check Windows image health"; risk = "Low · read-only health check; may take a few minutes."; break;
            case "sfc": file = "sfc.exe"; args = "/verifyonly"; description = "Verify protected system files without repairs"; risk = "Low · verification only; does not repair files."; break;
            default: return;
        }
        var approved = MessageBox.Show(this, $"Requested action: {description}\nProgram: {file}\nArguments: {args}\nRisk: {risk}\n\nAllow this action?", "Jeff action approval", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (approved != MessageBoxResult.Yes) { ApprovalStatus.Text = $"Denied: {description}"; return; }
        try
        {
            var psi = new ProcessStartInfo(file, args) { UseShellExecute = true, Verb = "runas" };
            Process.Start(psi);
            ApprovalStatus.Text = $"Approved and launched: {description}. Review the opened console for results.";
        }
        catch (Exception ex) { ApprovalStatus.Text = $"Not started: {ex.Message}"; }
    }

    private void LaunchProgram_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Choose a program to request permission for", Filter = "Programs (*.exe)|*.exe", CheckFileExists = true, Multiselect = false };
        if (dialog.ShowDialog(this) != true) return;
        var path = Path.GetFullPath(dialog.FileName);
        var approved = MessageBox.Show(this, $"Requested action: launch selected program\nProgram: {path}\nArguments: none\nRisk: Medium/unknown · the program may access files, network, or system resources under your Windows account. Only approve software you trust.\n\nAllow this program to launch?", "Jeff program approval", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (approved != MessageBoxResult.Yes) { ApprovalStatus.Text = "Program launch denied."; return; }
        try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); ApprovalStatus.Text = $"Approved launch: {path}"; }
        catch (Exception ex) { ApprovalStatus.Text = $"Launch failed: {ex.Message}"; }
    }

    private void OpenWindowsSecurity_Click(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo("windowsdefender:") { UseShellExecute = true }); }
        catch { try { Process.Start(new ProcessStartInfo("explorer.exe", "windowsdefender:") { UseShellExecute = true }); } catch (Exception ex) { MessageBox.Show(this, ex.Message, "Windows Security unavailable", MessageBoxButton.OK, MessageBoxImage.Warning); } }
    }

    private static string? FindMpCmdRun()
    {
        var roots = new[] { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows Defender"), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Microsoft", "Windows Defender", "Platform") };
        foreach (var root in roots)
        {
            if (!Directory.Exists(root)) continue;
            var files = Directory.GetFiles(root, "MpCmdRun.exe", SearchOption.AllDirectories);
            if (files.Length > 0) return files.OrderByDescending(File.GetLastWriteTimeUtc).First();
        }
        return null;
    }

    private async void ClamAvFileScan_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Choose a file for ClamAV scanning", CheckFileExists = true, Multiselect = false, Filter = "All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) != true) return;
        var approved = MessageBox.Show(this,
            $"Scan this file with the installed ClamAV engine?\n\nTarget: {dialog.FileName}\n\nClamAV signatures must already be installed and up to date. Jeff will not delete or quarantine anything.",
            "Approve ClamAV file scan", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (approved != MessageBoxResult.Yes) { EngineStatus.Text = "ClamAV file scan denied."; return; }

        EngineStatus.Text = "ClamAV is scanning the selected file…";
        try
        {
            var result = await new ClamAvScanner().ScanFileAsync(dialog.FileName);
            var message = $"ClamAV result: {result.Status}\nTarget: {result.TargetPath}\nExit code: {result.ExitCode}\nElapsed: {result.Duration:hh\\:mm\\:ss}\n\n{result.Output}";
            EngineStatus.Text = message;
            MessageBox.Show(this, message, "ClamAV scan result", MessageBoxButton.OK,
                result.Status == ClamAvScanStatus.ThreatFound ? MessageBoxImage.Warning :
                result.Status == ClamAvScanStatus.EngineError ? MessageBoxImage.Error : MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            EngineStatus.Text = "ClamAV scan unavailable or failed: " + ex.Message;
            MessageBox.Show(this, EngineStatus.Text, "ClamAV scan unavailable", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void ClamAvFolderScan_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Choose a folder for recursive ClamAV scanning" };
        if (dialog.ShowDialog(this) != true) return;
        var approved = MessageBox.Show(this,
            $"Recursively scan files under this folder with ClamAV?\n\nFolder: {dialog.FolderName}\n\nThis can take a long time and use substantial CPU and memory. The installed ClamAV database must be current. No files will be removed or quarantined.",
            "Approve ClamAV folder scan", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (approved != MessageBoxResult.Yes) { EngineStatus.Text = "ClamAV folder scan denied."; return; }

        EngineStatus.Text = "ClamAV is scanning the folder recursively…";
        try
        {
            var result = await new ClamAvScanner().ScanFolderAsync(dialog.FolderName);
            var message = $"ClamAV result: {result.Status}\nTarget: {result.TargetPath}\nExit code: {result.ExitCode}\nElapsed: {result.Duration:hh\\:mm\\:ss}\n\n{result.Output}";
            EngineStatus.Text = message;
            MessageBox.Show(this, message, "ClamAV folder scan result", MessageBoxButton.OK,
                result.Status == ClamAvScanStatus.ThreatFound ? MessageBoxImage.Warning :
                result.Status == ClamAvScanStatus.EngineError ? MessageBoxImage.Error : MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            EngineStatus.Text = "ClamAV scan unavailable or failed: " + ex.Message;
            MessageBox.Show(this, EngineStatus.Text, "ClamAV scan unavailable", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

}
