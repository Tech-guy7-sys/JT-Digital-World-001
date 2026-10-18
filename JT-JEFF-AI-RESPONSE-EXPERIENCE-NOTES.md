# Jeff AI response experience update

This update improves the shared chat experience across JT app previews and the desktop Agent Jeff interface. It adds rotating, task-aware waiting messages; a progressive response reveal; and safe, dependency-free Markdown formatting for headings, lists, inline emphasis, and code blocks. The assistant system prompt already instructs Jeff AI to answer directly, adapt detail to the task, provide actionable technical guidance, and avoid claiming it is ChatGPT or Gemini.

## Important implementation note
The current desktop IPC/provider path returns the complete model response in one call. This patch progressively reveals that completed response so the UI feels more responsive, but it is **not true token-by-token model streaming**. True streaming requires provider streaming support and an Electron IPC event/channel to forward chunks as the model generates them.

## Verification
- Archive structure and ZIP integrity checked after editing.
- JavaScript syntax should be checked by the included `npm run check:syntax` when run in the project build environment.
- Full Windows installers and live UI have not been built or visually tested as part of this change.
