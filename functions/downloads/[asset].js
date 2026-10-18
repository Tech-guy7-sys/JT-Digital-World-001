const RELEASE_ASSETS = Object.freeze({
  "JT-Digital-World-Setup-0.1.0-x64.exe": {
    url: "https://github.com/Tech-guy7-sys/JT-Digital-World-001/releases/download/jt-desktop-v0.1.0/JT-Digital-World-Setup-0.1.0-x64.exe",
    type: "application/vnd.microsoft.portable-executable"
  },
  "JT-Digital-World-0.1.0-arm64.dmg": {
    url: "https://github.com/Tech-guy7-sys/JT-Digital-World-001/releases/download/jt-desktop-v0.1.0/JT-Digital-World-0.1.0-arm64.dmg",
    type: "application/x-apple-diskimage"
  },
  "JT-Digital-World-0.1.0-x64.zip": {
    url: "https://github.com/Tech-guy7-sys/JT-Digital-World-001/releases/download/jt-desktop-v0.1.0/JT-Digital-World-0.1.0-x64.zip",
    type: "application/zip"
  },
  "JT-Digital-World-0.1.0-x86_64.AppImage": {
    url: "https://github.com/Tech-guy7-sys/JT-Digital-World-001/releases/download/jt-desktop-v0.1.0/JT-Digital-World-0.1.0-x86_64.AppImage",
    type: "application/octet-stream"
  },
  "SHA256SUMS.txt": {
    url: "https://github.com/Tech-guy7-sys/JT-Digital-World-001/releases/download/jt-desktop-v0.1.0/SHA256SUMS.txt",
    type: "text/plain; charset=utf-8"
  },
  "Agent-Jeff-Setup-0.1.0-x64.exe": {
    url: "https://github.com/Tech-guy7-sys/jt-digital-world/releases/download/agent-jeff-v0.1.0/Agent-Jeff-Setup-0.1.0-x64.exe",
    type: "application/vnd.microsoft.portable-executable"
  },
  "Agent-Jeff-Windows-x64-0.1.0-portable.zip": {
    url: "https://github.com/Tech-guy7-sys/jt-digital-world/releases/download/agent-jeff-v0.1.0/Agent-Jeff-Windows-x64-0.1.0-portable.zip",
    type: "application/zip"
  },
  "Agent-Jeff-SHA256SUMS.txt": {
    url: "https://github.com/Tech-guy7-sys/jt-digital-world/releases/download/agent-jeff-v0.1.0/SHA256SUMS.txt",
    type: "text/plain; charset=utf-8"
  }
});

export async function onRequest({ request, params }) {
  if (request.method !== "GET" && request.method !== "HEAD") {
    return new Response("Method not allowed", {
      status: 405,
      headers: { Allow: "GET, HEAD", "Cache-Control": "no-store" }
    });
  }

  const assetName = params.asset;
  const asset = RELEASE_ASSETS[assetName];
  if (!asset) {
    return new Response("Download not found", {
      status: 404,
      headers: { "Content-Type": "text/plain; charset=utf-8", "Cache-Control": "no-store" }
    });
  }

  try {
    const upstream = await fetch(asset.url, {
      method: request.method,
      headers: { "User-Agent": "JT-Digital-World-Download-Gateway/1.0" },
      redirect: "follow"
    });

    if (!upstream.ok || !upstream.body) {
      return new Response("This download is temporarily unavailable. Please try again shortly.", {
        status: upstream.status === 404 ? 404 : 502,
        headers: { "Content-Type": "text/plain; charset=utf-8", "Cache-Control": "no-store" }
      });
    }

    const headers = new Headers({
      "Content-Type": asset.type,
      "Content-Disposition": 'attachment; filename="' + assetName.replace(/["\\\r\n]/g, "") + '"',
      "X-Content-Type-Options": "nosniff",
      "Cache-Control": "public, max-age=900, s-maxage=3600",
      "Referrer-Policy": "no-referrer"
    });
    const length = upstream.headers.get("content-length");
    if (length) headers.set("Content-Length", length);

    return new Response(request.method === "HEAD" ? null : upstream.body, {
      status: 200,
      headers
    });
  } catch {
    return new Response("This download is temporarily unavailable. Please try again shortly.", {
      status: 502,
      headers: { "Content-Type": "text/plain; charset=utf-8", "Cache-Control": "no-store" }
    });
  }
}
