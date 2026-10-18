# JT Digital World v51 production setup

## Cloudflare Pages
1. Deploy the `Website` directory with `public` as the output directory.
2. Bind a Cloudflare D1 database to the Pages Function variable `JT_AUTH_DB`. Cloudflare documents D1 bindings for Pages Functions.
3. Apply `db/auth-schema.sql` to that D1 database.
4. Set `JT_RELEASE_REPOSITORY` to `YOUR_GITHUB_OWNER/YOUR_REPOSITORY`.
5. Redeploy.

The download function discovers the latest GitHub Release asset instead of assuming a single hard-coded filename.

## Important
The website cannot create a user account until `JT_AUTH_DB` is actually bound. The site will show a clear configuration error instead of pretending authentication works.
