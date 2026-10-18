# Agent Jeff for Windows — native voice application

## Build locally
1. Install the .NET 8 SDK on Windows.
2. Open PowerShell in this directory.
3. Run `dotnet restore`, then `dotnet run`.
4. To publish: `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true`.

## Implemented in this build
- Installed Windows voice selection, preview, user-started speech recognition, editable transcript, and speech output.
- Stoppable always-on-top desktop companion after sign-in. It cannot overlay the secure Windows lock/sign-in screen.
- Jeff Security Center UI for Microsoft Defender status, requesting a Defender quick scan after confirmation, requesting a daily 2:00 AM scan schedule, and launching a small allowlisted set of read-only Windows diagnostics behind a permission prompt.
- Program launch requests use a file picker and show the exact executable path and a risk warning before launch.
- Actions are not silently authorized by speech recognition. Windows policy and privileges can block security operations; check results in Windows Security.

## Important security limitations
This is **not a replacement for Avast or Microsoft Defender** and is not yet an independent antivirus engine. It does not implement continuous file-system monitoring, behavioral detection, quarantine, ransomware protection, or guaranteed threat detection. The scan feature invokes the Defender command-line tool already installed by Windows. Defender status queries and scan scheduling depend on the local Windows edition, policy, installation paths, and privileges; scheduling may fail without administrator permission. Check the result in Windows Security. Do not disable existing antivirus protection.

Voice enrollment/speaker verification is not implemented. Speech recognition does not establish speaker identity. Do not use voice matching alone to authorize sensitive actions. No voice samples are saved by this app.

The diagnostic actions are a fixed allowlist; Jeff does not accept arbitrary shell text. Program launches can still be risky, so inspect the path and only approve software you trust. Some Windows diagnostics may show a UAC prompt. Jeff does not silently delete files or auto-remediate threats.

## Experimental local file triage
The Security Center now includes an initial `AntivirusEngine` file-triage component. It computes SHA-256 hashes and applies a small, visible set of heuristics to selected files (double-extension executable names, executables in common download/temp locations, and selected risky script-command patterns). A separate button requests a Microsoft Defender custom scan of a selected file after user approval.

This is **not a complete or independently reliable antivirus engine**. Heuristics are not malware signatures, can miss malware, and can produce false positives. A clean heuristic result never means a file is safe. The component does not execute, delete, or quarantine files. Continue to keep Microsoft Defender or another trusted antivirus enabled. A production-grade engine would require maintained malware signatures/reputation feeds, robust file-format parsers, real-time filesystem monitoring, safe quarantine/restore, update signing, tamper protection, and extensive adversarial testing.
