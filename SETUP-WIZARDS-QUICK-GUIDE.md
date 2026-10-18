# Setup Wizards Quick Guide

**Setup Center:** `Setup-Wizard/START-JT-SETUP.bat` — prepares the source tree and can build all nine installers.

**App installation wizards:** run the generated `.exe` files from `Build/release/individual/<app-id>/`. Each app uses the NSIS assisted installer configured for a visible wizard, installation path selection, and shortcuts.

**Desktop agent:** Agent Jeff is a standalone app with its own installer, separate from the website.

**Website:** deploy separately through Cloudflare Pages. The website is not counted as an app.

Do not expect `.exe` installers inside the source ZIP. Build them on Windows first, then publish the resulting files to your configured release repository.
