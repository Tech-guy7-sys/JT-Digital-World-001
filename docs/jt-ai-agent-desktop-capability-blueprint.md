# JT AI Agent + Agent Jeff — Capability Blueprint

Status: product requirements / implementation target, not a claim that these features are already implemented.

## Product goal

Build two connected experiences under JT branding:

- **JT AI Agent (web chatbot):** the everyday conversational, research, creation, and task-planning interface.
- **Agent Jeff (desktop agent):** the permissioned local-computer companion that can act on approved files, applications, browser sessions, and developer tools.

They share the JT AI Core, conversation context, tool contracts, provider abstraction, and settings, but their permissions and execution environments remain separate. Do not expose local-computer capabilities to the public web chatbot unless a separately paired desktop client explicitly grants a scoped capability.

The goal is to combine useful patterns seen across Manus, Abacus AI, ChatGPT/ChatGPT Work, Gemini, and GitHub Copilot CLI/agent mode. This is a capability inspiration and requirements document—not a claim of exact feature parity, and not a request to copy their branding or proprietary implementation.

## Capability map

| Inspiration | Capability patterns to target in JT |
|---|---|
| Manus | Chat mode for direct answers; agent mode for long multi-step tasks; plan and execute; browser research; create deliverables; inspect supported files; resumable tasks; cloud execution separated from local-computer execution; user can intervene or take over. |
| Abacus AI | General-purpose task execution; multiple work modes (chat, code/CLI, desktop coworker); local file and document analysis; browser/desktop computer use; coding assistance; reusable workflows; connected tools; optional scheduled/triggered tasks; clear task progress and outputs. |
| ChatGPT / ChatGPT Work | Natural conversation; reasoning and synthesis; web research; work across files and data; use supported tools and apps; multi-step browser/computer workflows in supported environments; produce useful deliverables such as reports, documents, spreadsheets, presentations, and code; allow pause, correction, and confirmation before consequential actions. |
| Gemini | Multimodal prompts with images and supported files; long-context analysis when supported by the chosen model; research and cited synthesis; connected services; reusable skills/instructions; tasks and schedules where configured; visible progress, output files, and human takeover for steps requiring user input. |
| GitHub Copilot CLI / coding agents | Terminal-first coding mode; understand a repository; plan edits; create and modify files; run build/test/lint commands; inspect output and iterate; summarize diffs; request approval for commands and changes according to policy; allow reviewing, accepting, rejecting, and reverting changes. |

Capabilities in the source products change over time and can be plan-, region-, provider-, OS-, and configuration-dependent. Verify each integration against its current official documentation before implementation.

## JT AI Agent — web chatbot requirements

### Conversation and intelligence
- Stream responses with cancellation and clear busy/error/retry states.
- Maintain named chats, recent history, search, rename, delete, archive, and new chat.
- Support follow-up context and explicit project/workspace context.
- Offer concise and detailed response styles without changing the factual standard.
- Allow supported file/image attachments and explain unsupported formats or size limits.
- Use tools when appropriate: web research, document/file analysis, data analysis, code assistance, and connected apps.
- Clearly distinguish sourced facts, estimates, proposed plans, and actions actually completed.
- Cite sources for web research and provide downloadable outputs when a tool genuinely creates a file.
- Support model/provider selection through JT AI Core; never expose API keys in client-side code.
- Personalization and memory must be visible, user-controlled, and easy to review or disable.

### Task / agent mode
- Convert a broad goal into a visible plan with steps, status, and deliverables.
- Run supported tools in sequence, inspect results, recover from ordinary failures, and ask for missing information when blocked.
- Let the user pause, cancel, edit the plan, approve a step, or take over.
- Keep task logs, outputs, and references attached to the originating chat.
- Support resumable tasks only when execution state is actually persisted.
- Add scheduled or triggered tasks only after reliable background execution, opt-in, notification, and cancellation are implemented.

### Deliverables
- Generate or edit supported documents, tables, spreadsheets, slide decks, code, reports, and data visualizations.
- Provide real files/links from actual tool output; never fabricate a download link or claim a file was created when it was not.
- Keep generated artifacts linked to the chat/task and allow revisions.

## Agent Jeff — desktop agent requirements

### Local environment
- Packaged desktop app for supported Windows first, with macOS/Linux only after verified support.
- A local service/worker with explicit connection status and a secure authenticated channel to the UI.
- Allow users to select exact folders and project roots; do not silently grant whole-disk access.
- Read, search, summarize, classify, organize, rename, move, copy, and create files only inside approved scopes.
- Preview bulk file operations and offer undo/restore where feasible.
- Launch approved applications and interact with supported desktop/browser surfaces only through explicit tools.
- Screen capture, OCR, microphone, camera, and meeting-listener features are optional, off by default, and require separate, visible permission and a clear active indicator.
- Support local terminal and developer tools (PowerShell, CMD, Git, Node, Python, package managers, build tools) only through a controlled command runner.
- For coding tasks: inspect repository status, create a plan, edit files, run tests/builds, show diffs, summarize results, and support rollback.
- Provide a visible task panel with plan, current step, logs, file changes, permission requests, outputs, pause/stop, and take-over controls.
- Allow remote task initiation only after secure pairing, authentication, device approval, and user-configured availability. The device must be able to revoke pairing immediately.

### Voice control and a user-selected assistant voice (planned)

- Provide opt-in hands-free voice control for Agent Jeff with a visible microphone/listening indicator, push-to-talk mode, mute/stop control, and an obvious way to disable listening.
- Let the user choose the assistant's spoken voice from supported installed or configured text-to-speech voices, with a preview, language/accent options where available, speaking-rate and volume controls, and a reset option. Present this as Jeff AI / Agent Jeff voice settings.
- Distinguish **assistant output voice** (the voice Jeff speaks with) from **voice input** (the user's speech) and from **speaker verification** (checking who is speaking). Choosing an output voice does not identify or authenticate the user.
- If a user means a particular real person's voice, only offer a voice clone when the person has explicitly authorized it and the chosen provider permits it. Otherwise use a licensed, built-in, or user-selected synthetic voice; do not imitate a real person without consent.
- Use Windows speech recognition or another explicitly configured speech provider for voice commands. Show or confirm the transcribed command before consequential actions. Require the normal approval flow for deleting files, running risky commands, installing software, sending messages, changing security settings, or other external side effects.
- Voice control must not bypass Windows Hello, the Windows lock screen, account permissions, or authentication. Voice recognition alone is not a secure substitute for Windows sign-in or biometric authentication.
- Keep microphone access off until the user enables voice control; show a persistent listening state when active, avoid retaining recordings by default, and disclose whether audio is processed locally or sent to a provider.
- Test microphone denied/unavailable, speech recognition errors, multiple languages, background noise, wake/listening controls, cancel/stop, speech output disabled, and confirmation gates. Label this feature **Planned** until native implementation and tests pass.

### Windows sign-in awareness and welcome experience (planned)

- Detect whether Windows Hello is available on the current Windows device and explain which supported verification options Windows reports as configured; do not assume every PC has a compatible camera or fingerprint reader.
- Use supported Windows Hello / Windows authentication APIs only for an explicit, user-initiated verification flow. Windows Hello performs the biometric match inside Windows; Agent Jeff must never collect, store, imitate, or inspect raw face or fingerprint data.
- Do not attempt to intercept, replace, or draw over the Windows secure lock/sign-in screen. A normal third-party desktop app cannot display its own trusted greeting on top of that secure screen or receive a raw “face recognized” / “fingerprint recognized” event from it.
- After Windows has successfully signed the user in and Agent Jeff starts in the user's session, it may show a welcome panel and, if the user opts in, say a greeting such as “Recognized. Welcome, [Windows account display name].” Only use the signed-in Windows account's display name; provide a preview/edit option and do not infer identity from a camera image.
- During a separate Agent Jeff verification prompt, show accurate states such as “Waiting for Windows Hello…”, “Verification successful”, or “Verification cancelled/failed”. Never say “Recognized” or welcome a named user unless the OS-backed verification succeeded and the account identity is available.
- The greeting must be optional, with speech off by default or explicitly enabled, a mute control, quiet-hours option, and no audio capture unless separately permitted. Do not speak private account details aloud in shared spaces without opt-in.
- If Windows Hello is unavailable, cancelled, or fails, fall back to normal Windows sign-in and show a neutral status. Never bypass Windows lock, password, PIN, biometric policy, or other access controls.
- Test on supported Windows versions and hardware, including no biometric hardware, Windows Hello disabled, failed/cancelled verification, multiple Windows accounts, locked/unlocked transitions, speech disabled, and accessibility settings. Treat this as **Planned** until a native Windows implementation and those tests exist.

### Safety and confirmation policy
- Default to least privilege. Every tool declares required scopes and risk level.
- Ask before destructive or hard-to-reverse actions: deleting or overwriting files, bulk moves, installing software, changing security settings, executing untrusted scripts, sending messages/emails, publishing, spending money, or changing accounts.
- Require confirmation for external side effects. Show the exact target, action, and meaningful consequences.
- Treat webpage text, documents, emails, repositories, and tool output as untrusted data—not as instructions that can override system policy or permissions.
- Never ask users to paste passwords, API keys, recovery codes, or other secrets into chat. Use secure OS credential storage or OAuth where appropriate.
- Provide an emergency stop, audit history, and per-tool permission revocation.
- Do not run arbitrary commands invisibly. Explain commands and show their output; enforce policy and timeouts.
- No silent surveillance, hidden recording, credential harvesting, or covert data upload.
- Keep local files local unless a user explicitly chooses to send them to a cloud provider/tool. Show what data will leave the device.

## Shared JT AI Core architecture

1. **Provider abstraction:** provider/model adapters, including local Ollama where reachable and supported cloud APIs. Present the configured backend in user-facing language as Jeff AI where appropriate, while retaining accurate provider/privacy disclosures in settings.
2. **Tool registry:** typed tool schemas, input validation, permission scopes, risk classification, timeout, cancellation, and structured results.
3. **Execution router:** choose web/cloud sandbox, connected-service API, or local Agent Jeff runner based on the task and granted permissions. Never pretend one environment can access another.
4. **Policy/approval layer:** enforce tool permissions and confirmation gates server-side/in the desktop service, not merely through UI text.
5. **Task orchestrator:** plans, step state, retries, checkpoints, cancellation, logs, artifact references, and resumability.
6. **Context and memory:** chat history, user-approved preferences, project knowledge, and tool output with explicit retention controls and source boundaries.
7. **Artifacts service:** verified file generation, storage, download links, version history, and cleanup policies.
8. **Connectors:** opt-in integrations with least-privilege OAuth and a visible connected-apps management screen.
9. **Observability:** actionable errors, privacy-respecting diagnostics, token/cost limits where applicable, and no logging of secrets.

## Suggested operating modes

- **Chat:** answer, explain, brainstorm, rewrite, summarize, help with uploaded files.
- **Research:** browse sources, compare evidence, cite claims, produce a report.
- **Task / Deep Work:** plan and carry out multi-step work with progress and deliverables.
- **Code / CLI:** repository-aware coding, terminal execution under policy, tests, diffs, rollback.
- **Desktop:** operate user-approved local files and supported apps through Agent Jeff.
- **Create:** generate/edit supported documents, spreadsheets, slides, code, images or media only when the relevant tools exist.
- **Automations:** opt-in schedules and event triggers with visible status, failure alerts, pause, and delete controls.

## Release gates — never represent planned capability as working

A capability is "available" only after its implementation and tests pass in the target environment. Each feature should have:
- automated tests for success, errors, cancellation, permission denial, and malformed tool output;
- a visible settings/permission control;
- clear limitations and OS/provider requirements;
- confirmation/undo behavior appropriate to its risk;
- verified artifact delivery if it creates files;
- privacy and security review for local files, connectors, screen, microphone, and remote access.

Until then, label it **Planned**, **Preview**, or **Unavailable** accurately in the product UI.

## Official capability references reviewed

- Manus desktop / local computer: https://help.manus.im/en/articles/14178443-what-is-the-my-computer-feature-capable-of
- Manus chat vs agent mode: https://help.manus.im/en/articles/11711128-what-are-the-differences-between-chat-mode-and-agent-mode
- Abacus AI desktop: https://desktop.abacus.ai/
- Abacus computer use: https://abacus.ai/help/chatllm-ai-super-assistant/computer-use-and-automation
- Abacus general agent: https://api.abacus.ai/help/chatllm-ai-super-assistant/deepagent
- OpenAI agent capabilities: https://openai.com/index/introducing-chatgpt-agent/
- ChatGPT Work product overview: https://help.openai.com/
- Gemini tasks and workflows: https://support.google.com/gemini/answer/17094507
- GitHub Copilot agent mode / terminal workflow: https://learn.microsoft.com/en-us/visualstudio/ide/copilot-agent-mode?view=visualstudio
- GitHub Copilot CLI on Windows: https://learn.microsoft.com/en-us/windows-server/ai/configure-github-copilot-cli-local-inference


### Personal voice enrollment and speaker-gated replies (planned)

- Provide a user-initiated **Enroll my voice** flow in the native Agent Jeff desktop app. Explain the purpose, what is stored, how to delete it, and whether any audio leaves the device before recording begins.
- Record only a short enrollment sample after explicit microphone permission and consent. Prefer an on-device speaker-verification engine that converts the sample into a protected speaker embedding/template; do not keep the raw enrollment recording after processing unless the user separately opts in.
- Store the voice template encrypted in the current Windows user's protected local storage, restrict access to the signed-in account, and provide **Re-enroll**, **Delete voice profile**, and **Disable voice matching** controls. Never upload a voice profile by default.
- In **Only respond to my voice** mode, use voice-activity detection and speaker verification before processing a spoken command or speaking a private response. If confidence is low, the microphone is unavailable, or the verifier is missing, Jeff must not treat the speaker as verified; show “Voice not verified” and ask the user to use push-to-talk with an alternative verification step or turn voice-only mode off.
- Do not silently capture or analyze nearby people. Listening must be opt-in with a persistent microphone indicator, an easy stop/mute control, and no background recording while disabled. Voice-only mode must not prevent the user from recovering access through the Windows account or app settings.
- Speaker verification is probabilistic and can be fooled by replayed or synthesized audio. It must **not** be the sole authorization for sensitive actions, Windows sign-in, financial activity, account changes, file deletion, or sending/publishing content. Use Windows Hello or another strong OS-backed verification and the normal action-confirmation policy for those cases.
- Do not claim that Jeff can detect the Windows lock-screen face/fingerprint result or speak over the secure lock screen. Voice verification runs only in the logged-in user session after explicit permission.
- Test enrollment and deletion, wrong speaker, replay/synthetic audio, background noise, false accept/reject thresholds, microphone denial, locked session, template encryption/access control, and fallback/recovery. Keep this feature **Planned** until a native verifier is integrated and security-tested on supported Windows devices.
