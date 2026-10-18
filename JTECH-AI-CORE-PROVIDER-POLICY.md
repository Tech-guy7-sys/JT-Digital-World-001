# JTech AI Core Provider Policy

End users see only **Jeff AI** and **JTech AI Core**. The implementation can use Google Gemini and OpenAI models behind the shared provider abstraction; provider names and routing remain out of normal end-user UI.

| Plan | Internal provider pool |
|---|---|
| Free | Gemini, local Ollama |
| Student | Gemini, local Ollama, MiniMax |
| Premium | Gemini, local Ollama, MiniMax, OpenRouter, Hugging Face, OpenAI |
| Pro | Gemini, local Ollama, MiniMax, OpenRouter, Hugging Face, Cloudflare AI, Groq, OpenAI |
| Business | Gemini, local Ollama, MiniMax, OpenRouter, Hugging Face, Cloudflare AI, Groq, OpenAI |

Current defaults (all can be overridden in deployment configuration): Gemini `gemini-3.8-flash` and OpenAI `gpt-6-astra`. These defaults were checked against the providers’ public model documentation on 2026-10-09; confirm availability for the actual API project before release.

## Response style

Jeff AI uses a provider-neutral response-quality prompt inspired by broad qualities users expect from modern assistants: direct answers, adaptable detail, clean structure, practical examples, careful synthesis, clear uncertainty, and honest reporting of what was and was not tested. It does not claim to be ChatGPT or Gemini. Response profiles available to apps are `balanced`, `concise`, `detailed`, `tutor`, `technical`, and `creative`. Apps may pass `preferences.responseStyle`, `preferences.language`, and optional `preferences.customInstructions` to the shared core. The OpenAI API is separate from a ChatGPT subscription and requires an OpenAI API key with API billing enabled. Gemini requires a Google AI API key. Configure keys as server environment secrets; never commit credentials or expose them in renderer/client code. Provider availability, model access, pricing, and quotas can change.
