(() => {
  "use strict";
  const status = document.getElementById("agent-jeff-release-status");
  if (!status) return;

  const downloads = [
    ["agent-jeff-setup-download", "/downloads/Agent-Jeff-Setup-0.1.0-x64.exe", "Download Agent Jeff installer (.exe)"],
    ["agent-jeff-portable-download", "/downloads/Agent-Jeff-Windows-x64-0.1.0-portable.zip", "Download Agent Jeff portable ZIP"],
    ["agent-jeff-checksums-download", "/downloads/Agent-Jeff-SHA256SUMS.txt", "Download SHA-256 checksums"]
  ];

  status.textContent = "Agent Jeff v0.1.0 · stable Windows x64 release";
  for (const [id, href, label] of downloads) {
    const link = document.getElementById(id);
    if (!link) continue;
    link.href = href;
    link.textContent = label;
    link.hidden = false;
    link.style.display = "flex";
    link.setAttribute("download", "");
  }
})();
