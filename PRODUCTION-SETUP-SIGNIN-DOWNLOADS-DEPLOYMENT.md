# JT Digital World — Production Setup and Deployment

## Website deployment on Cloudflare Pages

1. Push this project to a GitHub repository you control.
2. Cloudflare Dashboard → Workers & Pages → Create → Pages → Connect to Git.
3. Select the repository and set the Pages root directory to `Website`. For a static Pages project, set the build command to `exit 0` and the build output directory to `public`. Keep `Website/functions` at the Pages project root (alongside `public`) so Pages Functions are deployed. If your dashboard uses a different build setup, preserve that same output/functions layout.
4. Deploy and test the `pages.dev` address before attaching a custom domain.
5. Add your domain under Pages → Custom domains and complete the DNS steps.
6. Test `/api/config`, `/api/health` if present, `/signin.html`, `/download.html`, and the app routes.

## Sign-in database

1. Create a Cloudflare D1 database.
2. Execute `Website/db/auth-schema.sql` in its console.
3. In the Pages project, open Settings → Bindings → Add → D1 database binding.
4. Set the binding variable name to `JT_AUTH_DB` and select the database.
5. Redeploy. Check `/api/config` and confirm `authConfigured` is true.
6. Open `/api/config` and confirm `authConfigured: true`; if false, sign-in is intentionally disabled and the page now explains the missing D1 binding.
7. Test account creation, sign-in, sign-out, session persistence, invalid passwords, and rate limits over HTTPS.

Important: the source code cannot connect your Cloudflare account or create/bind D1 remotely. Until `JT_AUTH_DB` is bound to the deployed Pages project and the schema is applied, account sign-in cannot succeed. The page now diagnoses this condition instead of letting users submit a form that cannot work.

The repository cannot provision your Cloudflare account or database. Before public production, configure rate limiting, email verification and recovery flows, monitoring, privacy/legal pages, and backup/restore procedures. Never commit production credentials.

## App downloads

1. On Windows, run `Setup-Wizard\START-JT-SETUP.bat` and build the nine app installers.
2. Verify the real `.exe` artifacts in `Build\release\individual` and test them on a separate Windows user/profile.
3. Publish actual artifacts to a GitHub Release in the repository used for downloads.
4. In Cloudflare Pages environment variables, set `JT_RELEASE_REPOSITORY=OWNER/REPOSITORY`; or set `JT_RELEASE_BASE_URL` and `JT_RELEASE_VERSION` for a release host.
5. Redeploy and test `/api/releases/agent-jeff/windows`, `/api/releases/motion/windows`, and the other app routes. A missing artifact should be treated as a failed release, not a successful download.

## Installer wizards

Each individual Windows build is configured for an assisted NSIS installer (`oneClick: false`) with an installation-folder choice and Start Menu/Desktop shortcuts. Electron Builder supports these assisted installer options as documented at https://www.electron.build/docs/nsis/.

## What still requires your accounts and Windows machine

The source ZIP cannot create your domain, Cloudflare database, GitHub release, code-signing certificate, or signed executable installers by itself. Those steps must be completed and verified before the public website can offer working downloads.


## Jeff AI — ChatGPT/OpenAI + Gemini

The shared `Build/packages/jt-ai-core` provider abstraction supports both OpenAI's Responses API and Gemini's Interactions API. Defaults are set to `gpt-6.1-sol` and `gemini-3.8-flash` as of the 2026-10-09 update, and remain overrideable using `OPENAI_MODEL` and `GEMINI_MODEL`. Add `OPENAI_API_KEY` and `GEMINI_API_KEY` as protected server/runtime secrets in the relevant environment. Do not place provider API keys in website JavaScript, app renderer files, or the repository. OpenAI API access is billed separately from a ChatGPT subscription.
