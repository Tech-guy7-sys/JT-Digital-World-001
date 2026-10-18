# Website Jeff assistant, JT social logo, and JT Code extension

- Added a floating Jeff website assistant UI on the home page, with common-question fallback answers and an optional live AI backend.
- Added `Website/functions/api/jeff.js`. Configure Cloudflare Pages secrets/variables `JEFF_AI_API_URL`, `JEFF_AI_API_KEY`, and optionally `JEFF_AI_MODEL` to an authorized OpenAI-compatible chat-completions endpoint. The browser never receives the API key. If not configured, the widget answers common JT FAQs locally.
- Added a JT SVG logo/favicon and 1200x630 social preview card; home page includes Open Graph and Twitter card metadata. Replace `https://jtechdigitalworld.com/assets/jt-social-card.png` with the real canonical production domain before publishing if different.
- Added a VS Code-compatible starter extension scaffold in `JT-Code-VSCode-Extension`. It is not yet published, packaged, or production-verified.
- Validation performed: ZIP integrity; JavaScript syntax checks for the website widget, Cloudflare function, and extension source. No live deployment or end-to-end AI test performed.
