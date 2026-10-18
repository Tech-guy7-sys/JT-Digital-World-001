# JT Digital World v60.3 — Release Download Setup

The public website download buttons resolve to signed desktop installers. The source package intentionally does not contain final `.exe`/`.dmg` files.

## Required production configuration

Set one of these Cloudflare Pages environment variables:

- `JT_RELEASE_REPOSITORY=OWNER/REPOSITORY` — recommended; the endpoint resolves the latest GitHub release and selects the matching per-app installer.
- `JT_RELEASE_BASE_URL=https://your-release-host.example/releases` together with `JT_RELEASE_VERSION=60.3.0` — alternate release host.

## Required Windows artifact names

The per-app builders use names such as:

- `Agent-Jeff-60.3.0-x64.exe`
- `JT-jeff-ai-60.3.0-x64.exe`
- `JT-motion-60.3.0-x64.exe`
- `JT-studio-60.3.0-x64.exe`
- `JT-design-60.3.0-x64.exe`
- `JT-code-60.3.0-x64.exe`
- `JT-cloud-60.3.0-x64.exe`
- `JT-app-manager-60.3.0-x64.exe`
- `JT-stream-60.3.0-x64.exe`

The macOS builders use the corresponding `.dmg` names.

## Why a website can show “file not found”

A website link cannot create an installer. If the GitHub release or release host does not contain the requested signed installer, the API now returns a clear diagnostic response instead of redirecting to a guessed/nonexistent file.

After the signed installers are built and published, the Windows/macOS buttons will resolve to the actual files.
