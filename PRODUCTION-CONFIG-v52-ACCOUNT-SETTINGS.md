# JT Account Settings — deployment notes

The website now includes `account-settings.html`, an Edge-inspired account settings area for profile name, theme preference, optional product updates, diagnostics preference, password changes, and sign-out. The page is mirrored in `public/`.

## Cloudflare Pages setup

1. Bind the Cloudflare D1 database as `JT_AUTH_DB`.
2. Apply the full `db/auth-schema.sql` to the same database. It is safe to re-run because tables use `CREATE TABLE IF NOT EXISTS`; the new `account_preferences` table stores theme and optional privacy preferences.
3. Deploy the site and test account creation, sign-in, `/account-settings.html`, profile changes, theme preference, privacy preferences, password changes, and sign-out over HTTPS.
4. The site keeps the email address read-only because changing it safely should include email verification.

The preferences are stored server-side in D1. They are not automatically synchronized into every desktop app yet; desktop account device-flow and website cookie sessions are separate integration paths. Each app must use the same supported account API/device flow to synchronize these preferences.
