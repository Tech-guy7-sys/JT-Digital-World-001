# ClamAV integration for Agent Jeff

Agent Jeff can invoke the separately installed ClamAV `clamscan.exe` engine for a selected file or recursively selected folder. It uses ClamAV's documented exit codes: `0` means no detection reported, `1` means one or more detections, and `2` means an engine or scan error. The app does not delete or quarantine files.

## Install and prepare ClamAV

1. Download ClamAV only from the official project page: https://www.clamav.net/downloads
2. Install the Windows x64 package appropriate for your PC.
3. Follow the official Windows setup instructions: https://docs.clamav.net/manual/Installing.html
4. Configure the signature database and run the official updater (`freshclam.exe`) according to the ClamAV documentation before scanning.
5. Open Agent Jeff → Jeff Security Center → use one of the **ClamAV scan** buttons.

Agent Jeff looks for `clamscan.exe` in common ClamAV install folders and on the current user's `PATH`. If it cannot find the executable, the app reports that no scan was started.

## Important limitations

- A successful scan means only that the installed ClamAV engine did not report a detection for the scanned target using its available signatures and configuration. It does not prove a file is safe.
- Keep ClamAV signatures updated. Agent Jeff does not silently install ClamAV, run `freshclam`, or claim the database is current.
- Microsoft Defender should remain enabled. This integration is on-demand scanning, not real-time protection or a replacement for a full endpoint-security suite.
- Agent Jeff does not automatically delete, move, or quarantine detections. Review results in context and use Windows Security for additional investigation.
- ClamAV is licensed under GPL-2.0. Review the license and distribution obligations before bundling or redistributing ClamAV binaries with Agent Jeff. This integration launches an external executable; it does not bundle those binaries.
- The official documentation recommends at least 3 GiB of RAM for ClamAV on Windows 10/11 and about 5 GiB of free disk space for the application and database.

Official references:
- Scanning usage and exit codes: https://docs.clamav.net/manual/Usage/Scanning.html
- License and supported platforms: https://docs.clamav.net/Introduction.html
