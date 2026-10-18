# JT AI provider gateway (Cloudflare Pages Functions)

The `POST /api/jeff` endpoint is a server-side provider gateway. It remains disabled unless explicitly enabled and all security gates pass.

## Supported provider adapters
- **OpenAI**: secret `OPENAI_API_KEY`; optional `OPENAI_BASE_URL` and `OPENAI_MODEL`. The JT AI Hub default is GPT Luna (`gpt-6-luna`).
- **Ollama / Jeff AI**: `OLLAMA_BASE_URL` and optional `OLLAMA_MODEL`. Cloudflare cannot reach a user's PC localhost; use a secured, reachable gateway. Never expose an unauthenticated Ollama port.
- **Anthropic**: secret `ANTHROPIC_API_KEY`; optional `ANTHROPIC_MODEL`.
- **Gemini**: secret `GEMINI_API_KEY`; optional `GEMINI_MODEL`. The JT AI Hub default is Gemini 2.5 Flash (`gemini-2.5-flash`).
- The free Microsoft Copilot consumer app does not expose a documented general-purpose chat API for this integration. The UI uses manual handoff instead.

## Required security setup before enabling
1. Keep `JEFF_API_ENABLED=false` or unset until all steps below are complete.
2. Protect the deployed JT AI Hub/API with a Cloudflare Access application and an allow policy limited to approved users. Configure the app so requests to `/api/jeff` receive the `Cf-Access-Jwt-Assertion` header.
3. In Pages Functions environment variables, set `JEFF_ACCESS_TEAM_DOMAIN` to the Access team domain hostname (for example, `team-name.cloudflareaccess.com`, without a scheme) and `JEFF_ACCESS_AUD` to the exact application AUD value shown by Cloudflare Access. Do not guess the AUD.
4. Create a Cloudflare Workers rate-limit binding named `JEFF_RATE_LIMITER` for the Pages Function. Use a conservative per-IP threshold appropriate to the expected traffic. If this binding is absent or unavailable, the endpoint deliberately returns 503. Confirm the exact binding configuration supported by the project's Cloudflare Pages deployment method before enabling.
5. Add only the provider secrets you intend to use in Cloudflare Pages → Settings → Variables and Secrets. Never commit secrets or put them in browser JavaScript.
6. Only after the Access policy, JWT settings, rate limiter, provider secrets, and monitoring are verified should you set `JEFF_API_ENABLED=true`.
7. Test an unauthenticated request (must return 401), a burst of requests (must return 429 when the limit is reached), a request without the rate-limit binding (must return 503), and one short request for each configured provider.

## Implemented protections
- Same-origin and JSON content-type checks.
- Request-body limit of 32 KB, message limit of 6,000 characters, and bounded chat history.
- Cloudflare Access JWT signature validation against the team's published RSA keys, including issuer, audience, expiry, and not-before checks.
- Required rate-limit binding. Missing configuration or limiter failure is fail-closed.
- Provider allow-list, server-only credentials, provider timeout, generic upstream errors, and no-store JSON responses.

## Limitations
- A website deployment does not prove that any AI provider is configured or reachable.
- API usage is billed separately by the respective provider.
- A hosted Pages Function cannot run a Windows CLI or connect to a user's local Ollama instance.
- A Cloudflare Access login is not a substitute for provider quotas, budget alerts, monitoring, and incident response. Keep the service disabled until these are in place.
- The website CI runs validation tests; it does not validate live provider credentials.
