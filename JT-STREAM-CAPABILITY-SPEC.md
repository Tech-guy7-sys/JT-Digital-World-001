# JT Stream — creator studio capability specification

**Status:** Product requirement / roadmap, not a claim that these features are already implemented.  
**Product:** JT Stream  
**Brand:** JT / J Tech Digital World  
**Platform priority:** Windows desktop first, with architecture that can later support macOS and supported mobile companion workflows.

## Product direction

JT Stream should deliver a creator-focused studio with a capability set comparable to Camo Studio, while remaining an original JT product rather than copying Camo's branding, interface, assets, or implementation. It should combine camera control, scene composition, broadcast production, audio mixing, recording, and creator workflow tools in one coherent workspace.

## Capability requirements

### 1. Camera and video inputs
- Discover and select built-in cameras, USB webcams, capture cards, DSLR/mirrorless camera inputs exposed by the OS, and supported virtual cameras.
- Plan for a supported Android/iOS companion-camera workflow; do not claim phone-camera transport until pairing, permissions, transport, and reconnect behavior are implemented and tested.
- Per-source resolution, frame rate, aspect ratio, orientation, crop, position, scale, and device selection where the hardware/OS exposes the capability.
- Image controls where supported: exposure, white balance, brightness, contrast, saturation, sharpness, and focus.
- Clearly report unavailable controls rather than showing fake working controls.

### 2. Scenes, layers, and overlays
- Multi-scene workspace with named scenes, thumbnails, collections, and fast scene switching.
- Drag/resize/align layers on a canvas; support video, image, text, shapes, logo/brand marks, backgrounds, and browser/web overlays.
- Lower-thirds, titles, watermarks, countdowns, alerts, and configurable overlay templates.
- Layer grouping, ordering, opacity, alignment guides, safe areas, and scene preview.
- Scene transitions with configurable duration; include cut/fade and expand only after rendering performance is tested.
- Save/import/export JT scene profiles using a documented versioned format.

### 3. Camera effects and background
- Color correction and LUT/profile support.
- Background blur/replacement only when a supported model, API, or external provider is actually available.
- Optional framing assistance and portrait effects behind explicit user controls.
- Provide a privacy indicator when camera/microphone capture is active and a one-click stop control.

### 4. Audio mixer
- Multiple selectable microphone, supported application-audio, system-audio, capture-device, and media-file sources as allowed by the OS.
- Per-source meter, gain/volume, mute, monitor, and sync delay where supported.
- Noise reduction and a master EQ/limiter, implemented using real audio processing rather than decorative controls.
- Master mix output routed to recording and streaming; investigate a virtual microphone/output driver separately and disclose driver installation requirements.
- Show permission/device errors and audio clipping warnings.

### 5. Recording and streaming
- Local recording with selectable destination, format/quality presets, recording timer, free-space checks, and recovery-safe stop/finalization.
- RTMP/RTMPS destination profiles with secure credential storage; never put stream keys in logs, URLs shown in public UI, source control, or crash reports.
- Stream health panel with connection state, bitrate, dropped frames, duration, and actionable error messages where measurable.
- Multiple destinations only after the encoder, bandwidth, licensing, and platform constraints are validated.
- Streaming and recording must require an explicit user action; display a persistent live/recording state.

### 6. Web, chat, and audience layers
- Browser-source layer for trusted overlay URLs, with navigation restrictions and clear permissions.
- Chat and alert overlays through documented platform APIs or authorized integrations only.
- Platform support must be listed only after integration, authentication, and end-to-end testing are complete.
- Never ask users to paste platform passwords or stream keys into chat.

### 7. Profiles, controls, and workflow
- Saved profiles for resolution, frame rate, aspect ratio, audio mix, and destination selection.
- Configurable hotkeys and a Stream Deck-like control surface/integration where supported; avoid claiming native Stream Deck support before implementing and testing the integration.
- Session gallery with recording metadata, thumbnails, and export/open-folder actions.
- Run-of-show notes, scene checklist, recent-session summary, and reusable creator presets.

### 8. JT AI integration
- Use the shared JT AI Core provider abstraction and present the assistant as Jeff AI.
- AI can help plan a run of show, draft lower-thirds, suggest scene arrangements, summarize a recording's user-provided transcript, and troubleshoot diagnostics.
- Any AI-triggered scene change, device switch, recording, or stream start/stop must require user confirmation; streaming/recording must never start silently.
- Cross-app assistance must use explicit per-app permissions and auditable tool calls.

## UX direction

- Distinct JT Stream identity: premium dark broadcast control room, graphite/black surfaces, JT pink accent with cool violet secondary accents, clear live/recording indicators, readable meters, and dense-but-organized panels.
- Main workspace: preview canvas; scene strip; sources/layers list; audio mixer; stream status; recording controls; properties inspector.
- Use a guided setup wizard for devices, camera, audio, output profile, and test recording.
- Make empty states, permissions, unsupported hardware, and failed integrations explicit. Do not present roadmap mockups as working functionality.

## Architecture and safety
- Separate UI from media engine and device adapters.
- Keep provider/platform integrations behind interfaces so they can be independently tested.
- Store secrets in OS credential storage or a protected local secret store, never committed to Git.
- Use explicit camera/microphone consent, persistent capture indicators, clear retention paths, and user-controlled delete/export.
- Keep desktop device control within Agent Jeff; JT Stream only operates its own capture/production functions under its own permissions.
- Add unit tests for profiles, scene serialization, hotkeys, and release packaging; add integration tests for actual camera/audio devices and streaming/recording on supported Windows configurations.

## Acceptance gates before claiming feature completion
1. A real camera preview works on a supported Windows device.
2. Scene/layer edits persist after restart and render correctly.
3. Real audio sources produce metering and a recorded test mix; mute and gain controls affect output.
4. Recording produces a playable file and handles stop/error paths.
5. A test stream connects to a user-owned endpoint and reports measurable status.
6. Secrets are redacted from logs and never committed.
7. Permissions, live/recording state, and emergency stop are verified.
8. Build artifacts, checksums, release notes, and installation/uninstallation are tested before publishing a release.

## Competitive reference

Camo Studio is a capability reference for camera controls, scenes and overlays, image adjustments, profiles, recording/streaming, and audio mixing. JT Stream must implement its own original experience and use only supported APIs and appropriately licensed components. Reference: https://camo.com/studio
