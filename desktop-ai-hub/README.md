# JT AI Hub desktop app

This is a dedicated Electron desktop wrapper for the deployed JT AI Hub workspace.

## Build on Windows

Requirements: Node.js 22 or newer and npm.

```powershell
cd desktop-ai-hub
npm install
npm run package:win
```

The Windows installer is written to `desktop-ai-hub-artifacts` at the repository root.

## Important behavior

- The app loads the hosted JT AI Hub at `https://jt-digital-world-new.pages.dev/jt-ai-hub/`; it is not an offline app.
- The renderer has no Node.js integration, uses context isolation and sandboxing, and keeps navigation inside the JT AI Hub origin. External HTTPS links open in the system browser.
- ChatGPT API access requires server-side OpenAI configuration and separate API billing.
- Jeff AI requires a secured, reachable Ollama gateway. A hosted Pages Function cannot connect to a user's localhost Ollama server.
- The free Microsoft Copilot consumer app has no documented general-purpose chat API for this use; the UI uses a manual copy/open/paste handoff.
- The Jeff AI gateway must remain disabled until Cloudflare Access JWT validation, the rate-limit binding, and provider configuration are completed. Do not put provider API keys in the desktop app or browser code.
- The installer is unsigned and may trigger Windows SmartScreen.
