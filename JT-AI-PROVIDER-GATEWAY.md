# JT AI provider gateway (Cloudflare Pages Functions)

This adds a real same-origin `POST /api/jeff` handler. It is a provider gateway, not an authentication system and not a guarantee that any provider has been configured.

## Supported server-side adapters
- **Ollama**: `OLLAMA_BASE_URL`, optional `OLLAMA_MODEL` (default `deepseek-r1:1.5b`). A Cloudflare-hosted function cannot reach your PC's `127.0.0.1`. Use a secured, reachable gateway. Do not expose an unauthenticated Ollama port to the public internet.
- **OpenAI**: secret `OPENAI_API_KEY`; optional `OPENAI_BASE_URL` (defaults to OpenAI's API base) and `OPENAI_MODEL` (defaults to `gpt-4o-mini`).
- **Anthropic Claude**: secret `ANTHROPIC_API_KEY`; optional `ANTHROPIC_MODEL` (defaults to `claude-3-5-haiku-latest`).
- **Google Gemini**: secret `GEMINI_API_KEY`; optional `GEMINI_MODEL` (defaults to `gemini-3.8-flash`).
- **Copilot CLI**: deliberately returns a clear native-bridge-required response; a hosted Pages function cannot launch a CLI on the user's Windows machine.

## Deploy requirements
1. Keep `JEFF_API_ENABLED` unset or set to `false` during initial deployment. The endpoint deliberately returns HTTP 503 until you explicitly enable it.
2. The repository includes a GitHub Actions deployment workflow at `.github/workflows/deploy-cloudflare-pages.yml`. Add repository Actions secrets `CLOUDFLARE_API_TOKEN` and `CLOUDFLARE_ACCOUNT_ID`; the workflow creates and deploys the replacement project `jt-digital-world-new`. It builds with `npm run build:website` and publishes `dist`, while keeping Pages Functions in the root `functions/` directory.
3. In Cloudflare Pages → project → Settings → Variables and Secrets, first add the secrets for providers you intend to use. Set model names as non-secret environment variables if desired. Never commit API keys to Git.
4. Add `JEFF_API_ENABLED=true` only after you have reviewed the security limits below and configured a provider. Redeploy after configuring the environment.
5. Test with a short chat for every configured provider. The browser UI persists the selected preference and sends it to the API.

## Security and current limits
- Same-origin check, JSON-only request validation, input/history bounds, no-store responses, provider request timeout, and generic upstream errors are implemented.
- The endpoint does **not** implement account authentication, quotas, persistent cross-user rate limiting, billing controls, or abuse monitoring. Before public launch, put it behind an authenticated account/session and a durable rate limiter. Do not advertise it as production-safe for unrestricted anonymous use.
- Provider names are allow-listed. Provider credentials are read only from server environment bindings and never from browser input.
- Responses are non-streaming JSON to match the current JT AI Agent UI.

## Local testing
Use Cloudflare Pages' local development tooling after installing dependencies in a development environment. The website CI performs static checks and the deployment workflow uses Wrangler. A successful website deployment does not prove live Gemini connectivity; configure `GEMINI_API_KEY` as a Cloudflare Pages secret and test the provider before enabling the API.
