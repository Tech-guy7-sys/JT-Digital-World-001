# JT Digital World — App capability and integration matrix

**Updated:** 2026-10-18  
**Purpose:** Define product-specific capability targets and the work required to make them real. Commercial products are reference points only; JT must use original JT branding, original UX, and compliant integrations.

## Product principles
- One shared JT AI/Core architecture, but different specialist tools in each app.
- Use provider adapters and stable contracts instead of hard-coding one model vendor into every app.
- Keep credentials in a native secure store or server-side secret store. Never put API keys, OAuth refresh tokens, or stream keys in browser local storage, public source, logs, or generated release artifacts.
- Use least-privilege permissions, explicit consent for desktop/file/network actions, visible task plans, audit events, and an emergency stop in Agent Jeff.
- Clearly label working features, local prototypes, planned features, and unavailable integrations. Never simulate a successful connection.
- Build in stages; do not claim parity with a commercial product until feature-by-feature tests support that claim.

## App-by-app benchmark and capability targets

| JT app | Reference products / category | Target JT capabilities | Implementation status and next engineering work |
|---|---|---|---|
| **JT Stream** | OBS Studio, XSplit Broadcaster, Camo Studio | Multi-source camera/screen/media scenes; layer positioning; overlays/lower-thirds; hotkeys; transitions; local recording; audio mixer; profiles; virtual-camera output; RTMP/RTMPS; stream-health and multi-output workflows | Browser Stage 01 already supports camera and screen capture, camera/screen/PiP compositions, branded title overlay, microphone gain/metering, profile import/export, snapshots and local WebM recording. Add real hotkeys and fade transitions next. RTMP/RTMPS, reliable system/app audio, hardware encoding, virtual camera, multistream, advanced effects and device controls need a native Windows media engine and integration tests. |
| **JT Code** | OpenAI Codex, OpenCode, Qwen Code, GitHub Copilot CLI | Project-aware code editing; plan/build/review agents; repository search; diffs; terminal/test runner; LSP diagnostics; multi-agent tasks; model/provider switching; safe approvals; Git integration | Current browser HTML/CSS/JS playground is a prototype. Add task handoff packs and agent adapter contracts. Full project file access, terminal, LSP, Git operations and tool execution belong in a permissioned Electron/native runtime—not an unrestricted public webpage. |
| **JT Design** | CorelDRAW Graphics Suite; vector design and illustration tools | Vector canvas; editable paths and shapes; layers/groups; nodes; typography; swatches; symbols/components; brand kits; tracing/import/export; print formats; AI-assisted vector creation | Current starter exports SVG/PNG. Expand to actual path editing, selection, grouping, snapping, SVG import/export, reusable design tokens and tested formats. AI image/vector generation requires an explicitly selected service/model. |
| **JT Cloud** | OneDrive, iCloud Drive, Google Drive/Dropbox-style cloud file managers | Unified project spaces; local files; provider connectors; sync queue; version history; conflict handling; sharing/permissions; search; selective sync; offline status | Current project vault and metadata index are browser-local only. OneDrive needs Microsoft Graph OAuth with a registered app. iCloud Drive has no general public third-party file API equivalent to Microsoft Graph; start with supported OS file pickers/native integrations and clearly state limitations. Do not label a provider connected until OAuth and a real API call succeed. |
| **JT AI Agent / Jeff AI** | ChatGPT, Claude, Gemini; GitHub Copilot CLI for terminal-agent workflows | Conversation history; file/document analysis; multimodal inputs where provider supports them; tool use; research; structured outputs; projects; provider routing; specialist subagents; citations; export/import; cost and privacy controls | Current UI stores local chat history and calls `/api/jeff`, but a production endpoint is not present in the static site tree. Implement a server/native provider gateway with per-provider adapters, streaming, authentication, quotas, error handling, and explicit provider consent. “Microsoft CLI” needs clarification; GitHub Copilot CLI is the likely coding-agent product if that is what was intended. |
| **Agent Jeff (desktop agent)** | Hermes Desktop Agent, Abacus AI Desktop/CoWork | Windows app/file/browser workflows; scoped tools; plan/approve/execute/verify; persistent project memory; scheduled tasks; specialist agents; audit log; pause/stop; rollback where possible | Browser task planner is not desktop control. Build on the existing native Agent Jeff app and its permission/audit model. Native tool execution must verify the target and result, require approval for sensitive actions, and support emergency stop. |
| **JT AI Core** | Provider gateways and agent runtimes | Provider abstraction; model catalogue; local Ollama; cloud provider adapters; tool registry; app-scoped permission policy; health diagnostics; structured events; rate limits; fallback and telemetry controls | Current browser provider profile is a local prototype. Move secrets and enforcement to a trusted native/server boundary. Do not expose local Ollama or credentials to public browser origins. |
| **JT Studio** | Ableton Live, FL Studio, REAPER | Multitrack audio recording/playback; waveform editing; MIDI; tempo/grid; instruments/effects; automation; buses/routing; mixing; export/render; session recovery | Current workbench supports test tone, microphone meter/recording and session notes. The next phase is a native audio engine and real multitrack project format; browser notes are not a DAW. |
| **JT Video & Photo** | DaVinci Resolve, Adobe Premiere Pro, Photoshop/Lightroom category | Non-destructive timeline; media bins; multi-track video/audio; trim/split; transitions; titles; colour; image layers/masks; proxy media; export presets; autosave | Current media bin previews local media and exports a shot list. Build a native media pipeline (FFmpeg or a suitable supported engine), real timeline model, project persistence and render tests before advertising editing/export. |
| **JT App Manager** | Windows Package Manager/winget and established app-update managers | App catalogue; installed-version detection; signed manifests; update channels; checksums/signature verification; staged updates; rollback; release notes; uninstall and health checks | Current browser UI reads public GitHub releases only. Native installation/update operations need signed packages, verified manifests, rollback, and install/uninstall tests. |
| **JT Account** | Secure identity and account-management systems | Sign-in; profile; session/device management; MFA/recovery; consent; privacy/export/delete; subscription entitlements if billing is added | Static account pages are not proof of real authentication. Select and deploy an identity provider or implement a tested backend before presenting accounts as live. Never collect passwords into static HTML/local storage. |
| **JT Digital World website** | High-quality product hubs and documentation sites | Clear app catalogue; polished app pages; release/download centre; docs; support; privacy/security pages; responsive design; working navigation; public status and sandbox | Static Cloudflare Pages build exists; live deployment/domain are separate verification steps. Keep page claims aligned with working product status. |
| **JT Sandbox** | Interactive product demos and safe test workspaces | Per-app demos; sample projects; resettable state; isolated previews; capability flags; test scenarios; accessibility checks | Browser demos exist in several app workspaces. Make the sandbox explicit about local-only data, reset state, and unavailable native/cloud features. |

## Recommended delivery order

### Stage A — Make existing prototypes honest and testable
1. Add a JT Stream hotkey map and real cut/fade scene transitions; keep “broadcast unavailable” visible until a native output engine exists.
2. Add JT Code handoff prompts for Codex, OpenCode, Qwen Code and Copilot CLI without pretending the agents are embedded or authenticated.
3. Add provider connector contracts to JT AI Core and a clear disconnected/connected/unsupported state.
4. Add a JT Cloud connector registry with honest status, starting with OneDrive/Microsoft Graph; keep iCloud limitations explicit.
5. Improve per-app test assertions and record the resulting CI run.

### Stage B — Implement the trusted shared services
1. JT AI Core: provider gateway, model catalogue, tool schema, per-app policy enforcement, request limits, streaming, observability and secret management.
2. JT Account: real authentication and sessions; only then gate personal cloud sync and provider settings.
3. JT Cloud: OAuth connectors, file metadata and content permissions, sync queue, conflict/version management and audit history.
4. Agent Jeff: native tool broker, explicit approval, execution verification, cancellation/emergency stop and audit logs.

### Stage C — Native engines
1. JT Stream: native capture/mixing/encoding, RTMP/RTMPS, multistream, virtual camera and device controls.
2. JT Code: workspace file access, terminal/test runner, LSP, Git diffs and permissioned coding agents.
3. JT Design: vector/path editor and interoperable file formats.
4. JT Studio: multitrack audio/MIDI engine.
5. JT Video & Photo: timeline editing and export/render pipeline.

## Integration and security gates
- Never store provider API keys, OAuth refresh tokens, or stream keys in localStorage or committed source.
- OAuth must use registered client IDs, correct redirect URIs, state/PKCE where applicable, minimum scopes, and revocation handling.
- Browser prototypes may not execute arbitrary host commands or control Windows desktop applications.
- Native agent actions must be permission-checked on every call; the UI checkbox alone is not an authorization boundary.
- Streaming keys must be stored in the OS credential vault and redacted from diagnostics.
- Every native install/update must verify publisher/signature and checksums, and provide a rollback or recovery path where possible.
- CI passing verifies the tested checks only; it does not prove that Cloudflare is deployed, provider accounts are connected, or native media engines work.

## Official capability references
- OBS Studio: https://obsproject.com/
- XSplit Broadcaster: https://www.xsplit.com/broadcaster
- OpenCode: https://opencode.ai/
- OpenAI Codex CLI: https://help.openai.com/en/articles/11369540-using-codex-with-your-chatgpt-plan
- GitHub Copilot CLI: https://github.com/features/copilot/cli
- CorelDRAW Graphics Suite: https://www.coreldraw.com/en/product/coreldraw/
- Microsoft OneDrive developer platform: https://learn.microsoft.com/onedrive/developer/
- Hermes Desktop: https://hermes-agent.nousresearch.com/desktop
- Abacus AI Desktop: https://desktop.abacus.ai/
- Ableton Live: https://www.ableton.com/en/live/what-is-live/
