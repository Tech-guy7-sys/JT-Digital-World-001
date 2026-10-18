# JT Digital World installer build repair

## What the npm output means

Messages about `inflight`, `rimraf`, `glob`, and `boolean` are dependency deprecation warnings. They are not, by themselves, the installer failure. `added 284 packages` means npm finished installing that dependency tree; the build can still fail later in `electron-builder`.

## Changes in this package

- Windows build scripts now install dependencies with `--no-audit --no-fund` to avoid spending extra time on the npm audit/funding phase during installer setup. This does not suppress build errors.
- The batch scripts check that Electron and electron-builder are present before starting.
- Source checks run before the all-app build.
- Build failures leave the command window open and display the failing app and a one-app retry command, instead of closing immediately and hiding the useful error.
- The all-app builder runs from the project `Build` directory explicitly and stops on the first failing app.

## Build on Windows

1. Extract the ZIP to a normal writable folder, not inside the ZIP preview or `Program Files`.
2. Install a supported Node.js LTS release and reopen Command Prompt.
3. Double-click `Build\BUILD-ALL-JT-APPS.bat`.
4. The installers should appear under `Build\release\individual\<app-id>\`. App IDs are `agent-jeff`, `jeff-ai`, `video-photo`, `studio`, `design`, `code`, `cloud`, `app-manager`, and `stream`.
5. If it fails, leave the window open and copy the first `Error:` / `ERROR:` line and the following stack trace. The npm deprecation warnings alone are not the failure.

To build Agent Jeff only, run in Command Prompt:

```bat
cd /d "C:\path\to\JT-Digital-World\Build"
npm install --no-audit --no-fund
npm run build:app:win -- agent-jeff win
```

## Verification boundary

The changes improve diagnostics and dependency-install behavior. A Windows NSIS/portable installer must still be built on the user's Windows machine or Windows CI to verify the full native packaging process. A successful `npm install` alone is not proof that installers were produced.

## Additional Windows command-launch fix

The per-app builder now invokes `npm` and `npx` through a shell. On Windows, these commands commonly resolve to `.cmd` shims; calling those shims directly through `execFileSync` can fail before Electron Builder starts. The revised scripts use `execSync(..., { shell: true })` with only fixed, allow-listed app IDs and build targets.

This environment could not complete `npm install` within the available execution window, so no claim is made that Windows installers have been generated. Please run the batch file on Windows and share the first actual Electron Builder error if it still fails.


## Windows CLI resolution improvement

The individual-app build script now launches the locally installed electron-builder CLI through the current Node executable instead of calling `npx`. This avoids shell resolution issues with `npx.cmd` on Windows and avoids an accidental network fetch. The script reports the exact CLI path and exit code. Dependency deprecation warnings from npm are not, by themselves, build failures.
