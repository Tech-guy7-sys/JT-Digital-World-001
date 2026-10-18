# JT Digital World — Development Release Notes
**Date:** 2026-10-18  
**Status:** Source update; not a published binary release.

## Included
- JT Stream Studio scene hotkeys, cut/fade transition control, and existing local recording/snapshot workflows.
- JT Code coding-agent handoff brief builder for Codex, OpenCode, Qwen Code, and Copilot CLI.
- JT Cloud connector status panel for OneDrive, iCloud Drive, and future Drive support.
- JT AI Agent provider preference selector for local/Ollama, OpenAI, Claude, Gemini, and Copilot CLI bridge.
- Cloudflare Pages Function at `functions/api/jeff.js` for same-origin provider gateway requests.
- Provider-specific configuration errors, server-side secret usage, request validation, origin checks, timeouts, and gateway validation tests.
- CI checks for the website build, app integration surfaces, JT Stream DOM references, and JT AI gateway request validation.

## Configure before live AI testing
Configure the appropriate Cloudflare Pages environment variables/secrets for the provider you plan to use. Do not commit credentials. A cloud function cannot connect directly to Ollama on a developer computer's localhost; use a secured reachable gateway.

## Not included / not yet verified
- No new Windows installer or signed binary has been built by this source update.
- No Cloudflare Pages production deployment or provider smoke test is claimed here.
- The provider gateway still needs authenticated user sessions, durable rate limits/quotas, abuse monitoring, and operational review before unrestricted public use.
- OneDrive/iCloud sync, native coding-agent execution, Agent Jeff desktop control, and JT Stream RTMP/virtual camera/multi-stream broadcasting remain separate implementation work.
- Existing Agent Jeff v0.1.0 prerelease is not replaced by these website commits.

## Verification
CI workflow: https://github.com/Tech-guy7-sys/jt-digital-world/actions
