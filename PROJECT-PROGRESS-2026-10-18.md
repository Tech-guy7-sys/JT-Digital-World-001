# JT Digital World — project progress snapshot

**Snapshot date:** 2026-10-18  
**Source of truth:** GitHub repository and workflow results inspected on this date.  
**Important:** This document distinguishes committed source/configuration from verified deployment, published releases, and features that are only planned.

## Website and Cloudflare Pages
- Main website repository: https://github.com/Tech-guy7-sys/jt-digital-world
- Default branch: main.
- Website build command: `npm run build:website`.
- Build output: `dist`.
- Build script: `scripts/build-site.mjs`.
- Wrangler Pages output points to `./dist`.
- GitHub Actions website CI had successful runs on 2026-10-17 (runs 39052255044 and 39052251267); an earlier run in the same push sequence failed (39052242372). The latest observed runs succeeded, but local build reproducibility and live Cloudflare deployment still need their own verification.
- Cloudflare account connection/project: not verified as connected.
- Custom domain: not purchased or verified. `jtdigitalworld.com` remains a candidate only.
- Static build only: no root `functions/` directory was present in the inspected tree. Sign-in persistence, API routes, update manifests, billing, and cloud APIs must not be advertised as operational until implemented and tested.

## GitHub repositories and releases
- Repositories visible in the connected GitHub account: `jt-digital-world`, `agent-jeff`, `jeff-ai`, and `jt-studio`.
- An official Agent Jeff Windows x64 pre-release was published successfully on 2026-10-18: https://github.com/Tech-guy7-sys/jt-digital-world/releases/tag/agent-jeff-v0.1.0. The other three repositories (`agent-jeff`, `jeff-ai`, and `jt-studio`) still had no published releases when checked.
- The main repository includes a `JT App Releases` workflow and a separate Agent Jeff Windows installer workflow. Do not infer release availability from a workflow alone; verify the release list and attached assets.
- Existing release notes in the repository include versions through 62.0.0, but these files are source documentation, not proof of a published GitHub Release or verified installer.
- Existing Windows installer guidance requires actual build outputs, install/upgrade/uninstall testing, and checks before public release. Do not fabricate setup EXEs or claim unsigned builds are signed.
- The website CI latest successful run: https://github.com/Tech-guy7-sys/jt-digital-world/actions/runs/39052255044
- The most recent observed JT Command Agent daily health scan succeeded: https://github.com/Tech-guy7-sys/jt-digital-world/actions/runs/39115236222
- Agent Jeff's Windows build workflow has successfully built the WPF application, passed all 9 unit tests, published self-contained x64 files, compiled the setup EXE, generated the portable ZIP/checksums, passed silent install/uninstall smoke tests, and published the official pre-release: https://github.com/Tech-guy7-sys/jt-digital-world/releases/tag/agent-jeff-v0.1.0.

## JT AI gateway implementation — 2026-10-18
- Added root Pages Function `functions/api/jeff.js`, exposing same-origin `POST /api/jeff` for the JT AI Agent.
- Implemented provider adapters for Ollama (through a configured reachable gateway), OpenAI-compatible chat completions, Anthropic Messages, and Google Gemini generateContent. Provider keys are read only from server environment variables; Gemini credentials are sent in a request header rather than a URL.
- Added origin and content-type checks, JSON/request/history length validation, provider allow-list, timeout handling, generic upstream error handling, and no-store JSON responses.
- Added `JT-AI-PROVIDER-GATEWAY.md` documenting Cloudflare Pages deployment, environment configuration, and security limits.
- Added `scripts/test-jeff-api.mjs` for request validation; CI now syntax-checks the gateway and runs these tests.
- The gateway is not a complete public production service yet: account authentication, durable rate limits/quotas, abuse monitoring, and live provider verification remain required before open public access. Local Ollama requires a secured, reachable gateway; Cloudflare cannot call a user's PC localhost directly. Copilot CLI explicitly requires a native Agent Jeff bridge.
- CI validation runs are available at https://github.com/Tech-guy7-sys/jt-digital-world/actions. Do not label provider integrations live until secrets are configured and each provider is smoke-tested in the deployed Pages project.

## JT app capability expansion — 2026-10-18
- Added `JT-ECOSYSTEM-CAPABILITY-MATRIX.md` with product-by-product benchmarks, current implementation boundaries, security requirements, and staged engineering order for every JT app.
- JT Stream browser Studio now has Alt+1/2/3 scene hotkeys, Alt+S snapshot, Alt+R record/stop, and selectable cut/fade-to-black transitions. Existing camera/screen/PiP composition, title overlays, profile import/export, snapshot and local WebM recording remain. This does not add RTMP/RTMPS broadcast, virtual-camera output, multi-streaming, hardware encoding, or OBS/XSplit feature parity; those need a native media engine.
- JT Code now has an AI coding-agent handoff brief builder for Codex, OpenCode, Qwen Code and GitHub Copilot CLI. It can build/copy/download a task brief with safety and testing instructions; it does not run these agents or execute terminal commands.
- JT Cloud now shows an honest connector registry for OneDrive, iCloud Drive and a possible later Google Drive integration. OAuth, sync, uploads, conflict resolution and remote storage remain unimplemented. No credentials are collected by the prototype.
- JT AI Core now displays the intended compatibility targets: local Ollama, OpenAI/ChatGPT/Codex, Anthropic Claude, Google Gemini, and native coding-agent bridges. The provider list is a roadmap/diagnostic surface, not proof those providers are connected.
- JT AI Agent now has a locally persisted provider preference for local/Ollama, OpenAI/ChatGPT, Claude, Gemini and GitHub Copilot CLI. The selected preference is sent to `/api/jeff`; the static repository still has no verified production API route, and adapters must be deployed before changing providers actually works. “Microsoft CLI” was interpreted as GitHub Copilot CLI; if a different Microsoft CLI was intended, identify the exact product before implementing an adapter.
- CI now checks the new JT Stream controls and app connector/handoff panels. Workflow run for the latest app-panel changes succeeded: https://github.com/Tech-guy7-sys/jt-digital-world/actions/runs/39134645190. The workflow including the newest added checks is still being verified separately.
- All claims above are separated into working browser workflows vs planned native/cloud integrations. No public Cloudflare deployment was performed as part of these changes.

## JT Stream — new requirement
- JT Stream should have a creator-studio capability set comparable to Camo Studio, including camera inputs and image controls, scenes/layers/overlays, effects, multi-source audio mixing, recording, RTMP/RTMPS streaming, session profiles, transitions, web/chat layers, hotkeys/control-surface integration, and diagnostics.
- JT Stream must retain original JT branding and UI rather than copy Camo's assets or interface.
- A detailed specification is recorded in `JT-STREAM-CAPABILITY-SPEC.md`.
- A working browser-based Stage 01 studio now exists at `jt-apps/jt-stream/studio.html`. Implemented: camera/microphone capture, screen sharing, Camera/Screen/Picture-in-Picture layouts, branded title/lower-third overlay, 720p/1080p canvas options, microphone gain/meter, local profile save/import/export, PNG snapshots, and local WebM recording. These controls still require hands-on browser/device testing. Direct RTMP/RTMPS output, virtual camera, advanced effects/background replacement, chat/alert integrations and platform streaming integrations remain planned.

## Other JT apps — interactive local-first workspace update
- Added shared app workspace runtime at `jt-apps/workspace-tools.js` and linked it from JT AI Core, JT App Manager, JT Cloud, JT Code, JT Design, JT Video & Photo (existing internal route `jt-motion`), JT Studio, and Agent Jeff.
- JT AI Core: local provider/model/endpoint preference fields and app-tool permission profile. This is a local preference UI only; it does not connect to an AI backend or enforce operating-system permissions.
- JT App Manager: searchable/filterable app library with links and clear prototype/release status. Existing release-check UI remains separate; app install/update automation is not implemented.
- JT Cloud: create local project records, add file metadata references, delete projects and export workspace JSON. It does not upload files or provide remote cloud storage.
- JT Code: editable HTML starter, sandboxed iframe preview, local draft save and HTML download. It is not a full IDE and does not run terminal commands.
- JT Design: editable simple SVG artboard with shape, color, text and SVG export. This is a starter tool, not a full layer-based design suite.
- JT Video & Photo: storyboard/shot-list planning with scene duration, local saving and JSON export; video editing/rendering is not implemented.
- JT Studio: local audio-session/track planner with BPM, key, mix notes and JSON export; it does not yet record or mix audio.
- Agent Jeff page: published v0.1.0 release link and a desktop-action safety checklist. The checklist is informational and does not configure actual agent permissions.
- Updated the app directory to display JT Video & Photo branding. Added CI checks for shared workspace script syntax and all eight app workspace links/assets.
- Latest website CI run after the workspace additions: https://github.com/Tech-guy7-sys/jt-digital-world/actions/runs/39133054208 (build and verification steps passed; run was still finalizing when checked).
- All new app workspaces are explicitly local-first prototypes. Real shared AI, cloud sync, production video/audio processing, full IDE functionality, and secure desktop permission enforcement require separate backend/native implementation and acceptance testing.

## Category-specific app capability update — 2026-10-18
- Added shared category-specific tool layer at `jt-apps/pro-workspaces.js`, linked from JT AI Core, JT App Manager, JT Cloud, JT Code, JT Design, JT Video & Photo, JT Studio, Agent Jeff, and JT Stream.
- JT AI Core: local provider/model/base-URL profile, browser-side endpoint reachability check, and local tool-scope preference checklist. This is not an authenticated production AI gateway; provider access can be blocked by CORS, and scope choices are not enforced server-side.
- JT App Manager: public GitHub release feed, app catalogue search, and clear release/prototype descriptions. It does not install, update, sign, or roll back binaries.
- JT Cloud: local project vault, tags/notes, metadata-only file index, search and JSON manifest export. No remote upload or sync.
- JT Code: separate HTML/CSS/JavaScript editor fields, sandboxed iframe preview, browser-local draft save, standalone HTML export, and starter reset. Not a full IDE; no terminal or arbitrary host filesystem access.
- JT Design: editable vector starter with shape/background colours, text, position/size controls, local save and SVG/PNG export. Not yet a full professional design suite or collaborative canvas.
- JT Video & Photo: local media-bin metadata, browser preview of selected local images/video, shot labels, in/out points, notes, timeline overview and JSON shot-list export. Does not render an edited video or upload footage.
- JT Studio: audio test tone, microphone meter, local microphone recording/download, and track/BPM/key/mix-note session planner. It does not yet provide a full multitrack DAW, effects chain, MIDI arrangement, or mix engine.
- JT Stream: scene/rundown planner and production-pack export are added alongside the existing Stage 01 camera/screen studio. No RTMP/RTMPS broadcasting or virtual camera.
- Agent Jeff: task queue, user-confirmation reminder, status labels and JSON audit export. These records are a planner only and do not invoke Windows desktop actions or replace runtime permission enforcement.
- Updated website CI to syntax-check the shared pro workspace script and verify all nine app pages include it. Successful run: https://github.com/Tech-guy7-sys/jt-digital-world/actions/runs/39133524933.
- These are real committed browser-side workflows, not a claim of feature parity with commercial category leaders. Production AI, cloud sync, installer/update enforcement, full media editing, professional audio processing, and Windows desktop control need their own service/native implementations and tests.

## JT Stream Studio — Stage 01 implementation
- Added browser studio page: https://github.com/Tech-guy7-sys/jt-digital-world/blob/main/jt-apps/jt-stream/studio.html
- Added original responsive JT studio styling in `jt-apps/jt-stream/studio.css`.
- Implemented camera and screen source toggles, camera/screen/PiP scene composition, branded canvas overlays, audio gain/metering and an audio mix graph, local WebM recording, still PNG export, and local JSON profile save/import/export in `jt-apps/jt-stream/studio.js`.
- Changed the JT Stream overview hero to link directly to the working studio. Updated website CI to syntax-check the studio/download JavaScript and verify that the studio page, styles, and script exist in `dist`.
- Website CI passed after the studio updates: https://github.com/Tech-guy7-sys/jt-digital-world/actions/runs/39132666373. The latest check includes JavaScript syntax checks, static build-output checks and validation of all 41 statically referenced studio element IDs.
- This browser studio is Stage 01, not a Camo Studio replacement yet. Live streaming/RTMP output and full virtual-camera/effects/device control still need a native or media-engine implementation and real hardware/platform tests.

## Recent confirmed configuration commits (main website repo)
- `333954c` — added allowlisted static website build script.
- `2a426e5` — added website CI workflow.
- `ca150f3` — added Cloudflare/GitHub setup documentation.
- `45b386b` — added build output and secret exclusions to .gitignore.
- `3068797` — added the website build command to package.json.
- `4788fc4` — pointed Wrangler Pages at `./dist`.

## Next steps
1. Run `npm run build:website` locally and verify the expected files in `dist`.
2. Connect the repository to Cloudflare Pages, deploy, and test the `pages.dev` preview.
3. Buy/connect a custom domain only after checking spelling and renewal cost.
4. Human-test Agent Jeff on a normal Windows 11 account: first launch, key features, shortcuts, install path, upgrade behavior, and uninstall. The published pre-release is unsigned and remains an early release.
5. Continue JT Stream Stage 02: test browser behavior on actual Windows hardware, harden audio/recording edge cases, and then design the native/media-engine layer required for RTMP/RTMPS streaming and virtual-camera output.
6. Keep each app's GitHub Releases page honest: version tag, release notes, source archive, and only those platform installers that were actually built and tested.


## Additional progress — 2026-10-18
- Added `JT-STREAM-CAPABILITY-SPEC.md` (commit `fd5f5d3`) covering camera input and controls, scene/layer composition, overlays/effects, multi-source audio, recording, RTMP/RTMPS streaming, profiles, hotkeys, creator integrations, Jeff AI assistance, safety, and acceptance tests.
- Updated `jt-apps/jt-stream/index.html` (commit `061357e`) to show the expanded creator-studio roadmap and clearly label production features as planned rather than already working.
- Updated `.github/workflows/jt-app-releases.yml` (commit `2f7b290`) to build the actual site with Node 22 and `npm run build:website`, verify key output files, and package the contents of `dist` as a website ZIP. This fixes the previous workflow's mismatch with the repository's root-based site layout.
- Added this progress snapshot (commit `f5b0c2e`).
- Website CI for the Stream page commit completed successfully: https://github.com/Tech-guy7-sys/jt-digital-world/actions/runs/39128815646. The static build and required-output checks passed. This validates the static build, not a live Cloudflare deployment.
- Current status update: Agent Jeff `agent-jeff-v0.1.0` is now a published GitHub pre-release with a setup EXE, portable ZIP, and SHA-256 manifest. The other product repositories remain without published releases; this does not imply those apps are packaged.


## Agent Jeff Windows download and release verification — 2026-10-18
- Fixed the native Windows compile error in `desktop/AgentJeff/ClamAvScanner.cs` by importing `System.IO` (commit `691e927`).
- Added `installer/AgentJeff.iss` for a JT-branded Windows setup wizard with uninstall support and optional desktop shortcut (commit `2e2a7b9`).
- Hardened `.github/workflows/agent-jeff-desktop.yml` to restore/build/test the real .NET 8 WPF application; publish self-contained Windows x64; compile the installer; generate portable ZIP + SHA-256 checksums; run a silent install/uninstall smoke test; upload build artifacts; and publish an early pre-release.
- **Successful final run:** https://github.com/Tech-guy7-sys/jt-digital-world/actions/runs/39130783302 — build, 9 automated tests, self-contained publish, installer compilation, portable archive/checksum generation, install/uninstall smoke test, artifact upload and release publication all succeeded.
- **Published release:** https://github.com/Tech-guy7-sys/jt-digital-world/releases/tag/agent-jeff-v0.1.0
- The official release assets are:
  - `Agent-Jeff-Setup-0.1.0-x64.exe` — SHA-256 `06a05488b342d214ba5d52cb0e7a33f971fc19557661caa12ab5134ede671043`.
  - `Agent-Jeff-Windows-x64-0.1.0-portable.zip` — SHA-256 `a00d804c18b28c49454d74700ea8893c0f7ef1d50cd57ccbf72fd37e2590bceb`.
  - `SHA256SUMS.txt` — official checksum manifest.
- The successful run’s Actions artifact is available until 2026-11-17: https://github.com/Tech-guy7-sys/jt-digital-world/actions/runs/39130783302/artifacts/12138226338
- The setup and portable ZIP were downloaded into the working environment from the same successful run and verified against `SHA256SUMS.txt`.
- **Release status:** published prerelease, not a final stable release. The installer is unsigned; SmartScreen may show a warning. Automated CI covers build, 9 unit tests and install/uninstall packaging smoke tests; human acceptance testing of the app’s actual runtime features on a normal Windows 11 machine remains necessary.
- The website downloads page queries the real GitHub release API and now recognizes published prereleases. Website CI passed for the script and allowlist updates. Live Cloudflare deployment still has not been verified.

- Final download-page visibility fix: `release-downloads.js` now explicitly hides setup/portable/checksum links when no valid release asset is available, because the site CSS assigns `display:flex` to `.platform-button` and can override the browser's default `hidden` attribute. Latest website CI passed after this fix: https://github.com/Tech-guy7-sys/jt-digital-world/actions/runs/39131169043.
- Final release verification confirmed `agent-jeff-v0.1.0` is published (`draft=false`, `prerelease=true`) with all three assets present. The latest successful build run: https://github.com/Tech-guy7-sys/jt-digital-world/actions/runs/39130783302.


## Deployment status checkpoint — 2026-10-18
- The repository's Cloudflare Pages build is ready and CI verifies the new studio assets are copied to `dist/jt-apps/jt-stream/`.
- Latest Studio CI passed: https://github.com/Tech-guy7-sys/jt-digital-world/actions/runs/39132387107.
- A live Cloudflare Pages deployment still cannot be verified from the connected GitHub tool. The Cloudflare Pages project must be connected to this repository in Cloudflare Dashboard (or a Cloudflare API token/account ID must be configured as protected deployment secrets). No Cloudflare credentials have been placed in source control or chat.
- Deployment settings: repository root, build command `npm run build:website`, output directory `dist`, production branch `main`. The site can be verified only after the project dashboard supplies a real `*.pages.dev` URL.
