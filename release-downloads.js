(() => {
  "use strict";
  const status = document.getElementById("agent-jeff-release-status");
  if (!status) return;

  const setupLink = document.getElementById("agent-jeff-setup-download");
  const portableLink = document.getElementById("agent-jeff-portable-download");
  const checksumLink = document.getElementById("agent-jeff-checksums-download");
  const releasesPage = "https://github.com/Tech-guy7-sys/jt-digital-world/releases";
  [setupLink, portableLink, checksumLink].forEach((link) => {
    if (link) link.style.display = "none";
  });

  function unavailable(message) {
    status.textContent = message;
    [setupLink, portableLink, checksumLink].forEach((link) => {
      if (link) {
        link.hidden = true;
        link.style.display = "none";
      }
    });
  }

  async function loadPublishedRelease() {
    try {
      const response = await fetch(
        "https://api.github.com/repos/Tech-guy7-sys/jt-digital-world/releases?per_page=30",
        { headers: { Accept: "application/vnd.github+json" }, cache: "no-store" }
      );
      if (!response.ok) throw new Error("GitHub release API returned " + response.status);
      const releases = await response.json();
      const release = releases
        .filter((item) =>
          !item.draft &&
          /^agent-jeff-v\d+\.\d+\.\d+(?:[-+][0-9A-Za-z.-]+)?$/.test(item.tag_name)
        )
        .sort((a, b) => Date.parse(b.published_at || b.created_at) - Date.parse(a.published_at || a.created_at))[0];

      if (!release) {
        unavailable("No published Agent Jeff installer is available yet. This page will show verified downloads after a release is published.");
        return;
      }

      const assets = Array.isArray(release.assets) ? release.assets : [];
      const setup = assets.find((asset) => /^Agent-Jeff-Setup-.+-x64\.exe$/i.test(asset.name));
      const portable = assets.find((asset) => /^Agent-Jeff-Windows-x64-.+-portable\.zip$/i.test(asset.name));
      const checksums = assets.find((asset) => /^SHA256SUMS\.txt$/i.test(asset.name));

      if (!setup) {
        unavailable("Agent Jeff " + release.tag_name.replace(/^agent-jeff-v/, "") + " is published, but its Windows setup installer asset was not found. Check the official release page.");
        return;
      }

      status.textContent = "Agent Jeff " + release.tag_name.replace(/^agent-jeff-v/, "") + (release.prerelease ? " · Windows x64 pre-release" : " · Windows x64 release") + " · installer and checksums";
      setupLink.href = setup.browser_download_url;
      setupLink.textContent = "Download Windows installer (.exe)";
      setupLink.hidden = false;
      setupLink.style.display = "flex";
      setupLink.setAttribute("aria-label", "Download Agent Jeff Windows setup installer");
      if (portable) {
        portableLink.href = portable.browser_download_url;
        portableLink.hidden = false;
        portableLink.style.display = "flex";
      }
      if (checksums) {
        checksumLink.href = checksums.browser_download_url;
        checksumLink.hidden = false;
        checksumLink.style.display = "flex";
      }
    } catch (error) {
      unavailable("Could not check published assets right now. Use the official GitHub Releases page.");
      const fallback = document.querySelector('#agent-jeff-downloads a[href="' + releasesPage + '"]');
      if (fallback) fallback.textContent = "Open official GitHub Releases ↗";
    }
  }

  loadPublishedRelease();
})();