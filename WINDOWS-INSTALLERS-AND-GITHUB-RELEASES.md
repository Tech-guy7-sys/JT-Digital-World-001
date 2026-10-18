# JT Digital World — Windows installer builds and releases

## Current verified scope

The repository currently contains one native Windows desktop project: **Agent Jeff**, implemented in `desktop/AgentJeff/` using .NET 8 WPF. Its test project is `desktop/AgentJeff.Tests/`.

The other JT products shown on the website are not all packaged as native desktop applications in this repository. In particular, the earlier references to a `Build/` directory and a nine-app Electron installer suite do not match the current GitHub tree and must not be used as build instructions until that source is actually present.

## Agent Jeff Windows build pipeline

Workflow: [Build Agent Jeff Windows Installers](.github/workflows/agent-jeff-desktop.yml)

On pushes to `main` that modify the Agent Jeff source, its tests, the installer script, or this workflow, GitHub Actions runs on a Windows runner and:

1. Restores the .NET test project and its referenced application.
2. Builds the Agent Jeff WPF application.
3. Runs the automated unit tests.
4. Publishes a self-contained Windows x64 application.
5. Compiles the JT-branded Inno Setup installer.
6. Builds a portable ZIP and creates SHA-256 checksums.
7. Installs and uninstalls the setup package silently in the runner as a packaging smoke test.
8. Uploads the installer, portable ZIP and checksums as a 30-day GitHub Actions artifact.

Expected filenames:

- `Agent-Jeff-Setup-<version>-x64.exe` — Windows setup wizard with Start Menu entry and optional desktop shortcut.
- `Agent-Jeff-Windows-x64-<version>-portable.zip` — portable self-contained publish output.
- `SHA256SUMS.txt` — SHA-256 hashes for the setup and portable archive.

A successful pipeline validates build, automated tests, installer compilation, and basic install/uninstall behavior. It does not replace a human acceptance test of the app's launch, runtime features, upgrade behavior, security, or usability on a normal Windows 11 account.

## Publish an Agent Jeff pre-release

The workflow supports three release paths:

- Push a version tag such as `agent-jeff-v0.1.0` after reviewing the intended source commit.
- Open **GitHub → Actions → Build Agent Jeff Windows Installers → Run workflow**, enter a semantic version such as `0.1.0`, and set **Publish release** to true to publish the marked pre-release once all checks pass.
- Use the repository's explicitly prefixed release commit flow when a release run is deliberately prepared.

The release step runs only after all prior build, test, packaging, and install/uninstall checks succeed. It publishes an **early pre-release**, with the setup EXE, portable ZIP and SHA-256 checksums attached. The release body warns that the installer is unsigned and that CI package checks do not replace human feature testing.

For normal CI checks, leave the draft-release option off. Main-branch CI builds use a version resembling `0.1.0-ci.<run-number>` and upload an artifact without publishing a release.

## Current published Agent Jeff pre-release

**Published 2026-10-18:** [Agent Jeff v0.1.0 — Windows x64 pre-release](https://github.com/Tech-guy7-sys/jt-digital-world/releases/tag/agent-jeff-v0.1.0)

Assets attached to this GitHub Release:
- [Windows setup installer (.exe)](https://github.com/Tech-guy7-sys/jt-digital-world/releases/download/agent-jeff-v0.1.0/Agent-Jeff-Setup-0.1.0-x64.exe) — SHA-256: `06a05488b342d214ba5d52cb0e7a33f971fc19557661caa12ab5134ede671043`
- [Portable Windows ZIP](https://github.com/Tech-guy7-sys/jt-digital-world/releases/download/agent-jeff-v0.1.0/Agent-Jeff-Windows-x64-0.1.0-portable.zip) — SHA-256: `a00d804c18b28c49454d74700ea8893c0f7ef1d50cd57ccbf72fd37e2590bceb`
- [SHA-256 checksum manifest](https://github.com/Tech-guy7-sys/jt-digital-world/releases/download/agent-jeff-v0.1.0/SHA256SUMS.txt)

Verified GitHub Actions run: https://github.com/Tech-guy7-sys/jt-digital-world/actions/runs/39130783302. The .NET build and 9 automated tests passed. The installer compiled, the portable ZIP/checksums were created, and the installer was silently installed and uninstalled successfully on the Windows runner.

This is a **pre-release**, not a final stable release, and the installer is **unsigned**. Package and install smoke tests do not replace human testing of app features on a normal Windows 11 account.

## Installer behavior and security

- The setup requires administrator privileges and installs under the JT Digital World application folder in Program Files.
- The installer registers a normal Windows uninstaller and creates a Start Menu shortcut; a desktop shortcut is optional.
- Builds are currently unsigned unless a signing certificate and secure signing workflow are configured. Do not claim publisher verification or code signing. SmartScreen may show warnings for unsigned builds.
- Review SHA-256 checksums after downloading; checksums establish file integrity against the accompanying manifest, not the publisher's identity.
- A portable build does not install an uninstall entry or create Start Menu shortcuts.

## Other products and platform coverage

Do not advertise Windows installers for the other JT apps based only on website previews or roadmap notes. Each product needs a real buildable project, its own build/test pipeline, package verification, and a release artifact. No macOS installer is produced by this workflow.

## Website downloads page

The website's downloads page should continue to distinguish published releases from previews. Only show a setup download as available when a real asset appears in the relevant GitHub Release. A successful source build alone is not evidence that a public release exists.
