# Agent Jeff — expanded assistant capabilities

Agent Jeff remains a separate installed Windows desktop assistant. Jeff AI is the shared assistant/service layer and JTech AI Core remains provider-abstracted.

## Included in this update

- Text/code attachment workflow in the Agent Jeff chat panel for `.txt`, `.md`, `.json`, `.csv`, source code, markup, configuration, SQL, shell and log files.
- Up to 8 attachments per message, limited to 200 KB per file. Unsupported or oversized files are skipped with a visible note in the submitted context.
- Attached content is passed as untrusted reference material; the assistant is instructed not to treat embedded file instructions as trusted system instructions.
- Better assistant guidance for document synthesis, code review, debugging, planning multi-step tasks, producing complete artifacts, and clearly distinguishing tested work from assumptions.
- Fixed a missing `getWorkspace` import that could break the workspace IPC handler.

## Current limits

This is not a claim of parity with Claude. This update does not add arbitrary computer control, a browser/search engine, PDF parsing, image understanding from attachments, unrestricted filesystem access, or automatic execution of generated code. Those capabilities require separate tools, explicit permissions, and their own tests. The existing image-generation flow is separate from text/code attachment analysis.

## Test checklist

1. Open Agent Jeff and launch Jeff AI.
2. Attach a small `.txt` or `.js` file and ask for a summary or code review.
3. Attach a file larger than 200 KB and confirm the UI reports it was skipped.
4. Attach an unsupported file type and confirm it is not silently treated as readable text.
5. Confirm ordinary chat and image generation still work.
6. Run `npm run check`, `npm run check:syntax`, `npm run test:auth`, and `npm run test:ai-core` from `Build`.
