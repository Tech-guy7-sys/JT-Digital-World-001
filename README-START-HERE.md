# JT Digital World — Windows Setup Center

This package contains the JT Digital World website, all 9 JT applications, JT Command Agent, Update Manager, and the Windows build configuration.

## One-click start
Double-click `START-HERE.bat`.

## Build all Windows installers
Choose option 2, or run `Build\BUILD-ALL-JT-APPS.bat`.

The nine installers are generated under `Build\release\individual\`.

## Important
The package contains the real build configuration and source. Windows `.exe` installers are generated locally because Electron Builder must run on Windows for the Windows targets.

## Production services
Cloudflare Pages/D1, GitHub Releases, Stripe, OAuth credentials, AI provider keys, and Windows code-signing credentials must be configured separately for production deployment.
