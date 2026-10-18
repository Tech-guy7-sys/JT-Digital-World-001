# JT branded Windows installer welcome pages

All nine Windows NSIS installer configurations now include an app-specific JT-branded welcome page and header graphic. Each app has its own 164x314 sidebar bitmap and 150x57 header bitmap under `Build/build/`, plus a small NSIS include file. The NSIS include customizes the maintained electron-builder NSIS installer; it does not replace the built-in installer/uninstaller engine.

Apps covered: Agent Jeff, Jeff AI, JT Studio, JT Stream, JT Cloud, JT Design, JT Code, JT Motion, and JT App Manager. The app-specific welcome text is compiled into each installer. The uninstall wizard also receives a branded welcome page.

## Build and verify on Windows

1. From `Build`, run `npm install --no-audit --no-fund`.
2. Run `npm run check` and `npm run check:syntax`.
3. Run `npm run build:apps:win`.
4. Run `npm run verify:windows-installers`.
5. Test one installer from each app on a clean Windows 11 account: verify the welcome art, destination selection, shortcuts, launch, Installed Apps entry, and uninstall. Repeat for all nine before publishing.

The source ZIP cannot prove the Windows NSIS binaries compile or that the resulting installers work. A Windows build and manual install/uninstall test are still required.
