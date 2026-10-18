const { app, BrowserWindow, shell } = require("electron");

const HUB_URL = "https://jt-digital-world-new.pages.dev/jt-ai-hub/";
const HUB_ORIGIN = new URL(HUB_URL).origin;

async function createWindow() {
  const win = new BrowserWindow({
    width: 1480,
    height: 940,
    minWidth: 900,
    minHeight: 620,
    backgroundColor: "#090a12",
    title: "JT AI Hub",
    autoHideMenuBar: true,
    webPreferences: {
      contextIsolation: true,
      nodeIntegration: false,
      sandbox: true,
      webSecurity: true
    }
  });

  win.webContents.setWindowOpenHandler(({ url }) => {
    try {
      const target = new URL(url);
      if (target.protocol === "https:" && target.origin !== HUB_ORIGIN) {
        shell.openExternal(target.toString());
      }
    } catch {}
    return { action: "deny" };
  });

  win.webContents.on("will-navigate", (event, url) => {
    try {
      const target = new URL(url);
      if (target.origin !== HUB_ORIGIN) {
        event.preventDefault();
        if (target.protocol === "https:") shell.openExternal(target.toString());
      }
    } catch {
      event.preventDefault();
    }
  });

  await win.loadURL(HUB_URL);
}

app.whenReady().then(createWindow).catch((error) => {
  console.error("JT AI Hub could not start:", error);
  app.quit();
});
app.on("window-all-closed", () => {
  if (process.platform !== "darwin") app.quit();
});
app.on("activate", () => {
  if (BrowserWindow.getAllWindows().length === 0) createWindow().catch(() => app.quit());
});
