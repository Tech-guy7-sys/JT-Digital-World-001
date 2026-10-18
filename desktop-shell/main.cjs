const { app, BrowserWindow, shell } = require("electron");
const http = require("node:http");
const fs = require("node:fs");
const path = require("node:path");

let server;
let localOrigin;

const MIME = {
  ".html": "text/html; charset=utf-8",
  ".css": "text/css; charset=utf-8",
  ".js": "text/javascript; charset=utf-8",
  ".mjs": "text/javascript; charset=utf-8",
  ".json": "application/json; charset=utf-8",
  ".svg": "image/svg+xml",
  ".png": "image/png",
  ".jpg": "image/jpeg",
  ".jpeg": "image/jpeg",
  ".webp": "image/webp",
  ".ico": "image/x-icon",
  ".woff": "font/woff",
  ".woff2": "font/woff2",
  ".txt": "text/plain; charset=utf-8",
  ".webmanifest": "application/manifest+json"
};

function siteRoot() {
  return app.isPackaged
    ? path.join(process.resourcesPath, "site")
    : path.resolve(__dirname, "../dist");
}

function startLocalServer() {
  if (server && server.listening && localOrigin) return Promise.resolve(localOrigin);
  const root = siteRoot();
  if (!fs.existsSync(path.join(root, "home.html"))) {
    throw new Error("The packaged JT website files are missing. Rebuild the website bundle.");
  }

  server = http.createServer((req, res) => {
    let pathname;
    try {
      pathname = decodeURIComponent(new URL(req.url, "http://127.0.0.1").pathname);
    } catch {
      res.writeHead(400).end("Bad request");
      return;
    }

    if (pathname === "/api/jeff" || pathname.startsWith("/api/jeff/")) {
      res.writeHead(503, { "Content-Type": "application/json; charset=utf-8" });
      res.end(JSON.stringify({ error: "The cloud Jeff AI API is not available in the offline desktop bundle." }));
      return;
    }

    if (pathname === "/") pathname = "/home.html";
    let target = path.resolve(root, "." + pathname);
    if (target !== root && !target.startsWith(root + path.sep)) {
      res.writeHead(403).end("Forbidden");
      return;
    }

    try {
      if (fs.existsSync(target) && fs.statSync(target).isDirectory()) target = path.join(target, "index.html");
      if (!fs.existsSync(target) || !fs.statSync(target).isFile()) {
        res.writeHead(404, { "Content-Type": "text/plain; charset=utf-8" }).end("JT page not found");
        return;
      }
      res.writeHead(200, {
        "Content-Type": MIME[path.extname(target).toLowerCase()] || "application/octet-stream",
        "X-Content-Type-Options": "nosniff",
        "Cache-Control": "no-store"
      });
      fs.createReadStream(target).pipe(res);
    } catch {
      res.writeHead(500).end("Unable to open this JT workspace");
    }
  });

  return new Promise((resolve, reject) => {
    server.once("error", reject);
    server.listen(0, "127.0.0.1", () => {
      const address = server.address();
      localOrigin = "http://127.0.0.1:" + address.port;
      resolve(localOrigin);
    });
  });
}

async function createWindow() {
  const origin = await startLocalServer();
  const win = new BrowserWindow({
    width: 1440,
    height: 920,
    minWidth: 920,
    minHeight: 640,
    backgroundColor: "#080914",
    title: "JT Digital World",
    webPreferences: {
      contextIsolation: true,
      nodeIntegration: false,
      sandbox: true
    }
  });

  win.webContents.setWindowOpenHandler(({ url }) => {
    if (url.startsWith("https://") || url.startsWith("http://")) shell.openExternal(url);
    return { action: "deny" };
  });
  win.webContents.on("will-navigate", (event, url) => {
    if (!url.startsWith(origin + "/")) {
      event.preventDefault();
      if (url.startsWith("https://") || url.startsWith("http://")) shell.openExternal(url);
    }
  });
  await win.loadURL(origin + "/home.html");
}

app.whenReady().then(createWindow).catch((error) => {
  console.error("JT Digital World failed to start:", error);
  app.quit();
});
app.on("window-all-closed", () => {
  if (server) server.close();
  if (process.platform !== "darwin") app.quit();
});
app.on("activate", () => {
  if (BrowserWindow.getAllWindows().length === 0) createWindow().catch(() => app.quit());
});
