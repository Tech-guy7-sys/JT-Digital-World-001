# JT Digital World v50 production configuration

## 1. Website sign-in
v50 replaces the previous placeholder sign-in page with a real Cloudflare Pages + D1 account flow.

Create a Cloudflare D1 database and bind it to the Pages project as:
`JT_AUTH_DB`

Apply `Website/db/auth-schema.sql` to that database.

The website then provides:
- `/api/auth/signup`
- `/api/auth/signin`
- `/api/auth/session`
- `/api/auth/signout`

Sessions use an HttpOnly, Secure, SameSite cookie. Passwords are PBKDF2-derived; plaintext passwords are never stored.

## 2. App downloads
Preferred configuration:
`JT_RELEASE_REPOSITORY=YOUR-GITHUB-OWNER/YOUR-GITHUB-REPOSITORY`

The download function reads the latest public GitHub release and resolves the exact asset name for each app.

The release artifacts use:
`JT-<app-id>-<version>-x64.exe`
`JT-<app-id>-<version>-x64.dmg`

Fallback configuration:
`JT_RELEASE_BASE_URL=https://github.com/YOUR-OWNER/YOUR-REPOSITORY/releases/download/app-v50.0.0`
`JT_RELEASE_VERSION=50.0.0`

Cloudflare Pages environment variables are configured under Settings > Variables and Secrets. Do not put passwords, database credentials, OAuth secrets, or API keys in website source files.
