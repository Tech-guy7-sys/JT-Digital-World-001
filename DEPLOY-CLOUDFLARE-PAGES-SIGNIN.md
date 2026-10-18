# Deploy the JT Digital World website and enable sign-in

The website is not live until it is deployed to a Cloudflare account. The repository cannot create a Cloudflare account, approve access, or supply a real D1 database ID on your behalf.

## 1. Create the Pages project

1. Sign in to Cloudflare Dashboard.
2. Open **Workers & Pages** and create a Pages project connected to the JT Digital World Git repository, or use Wrangler from this `Website` directory.
3. Use `Website/public` as the static output directory. Keep the Pages Functions in `Website/functions` so `/api/*` routes are deployed with the site.
4. Set the custom domain only after the first deployment succeeds.

## 2. Create and initialize D1

1. In Cloudflare Dashboard, open **Storage & databases → D1 SQL Database** and create a database for JT Digital World authentication.
2. Open the database console and execute the full contents of `Website/db/auth-schema.sql`.
3. Copy the real database ID from Cloudflare.
4. Add a D1 binding to the Pages project with binding name exactly `JT_AUTH_DB`, selecting the database created above. Configure it for both Preview and Production environments if both are used.
5. Redeploy after changing bindings.

Do not put database credentials, API tokens, or other secrets into client-side HTML or commit them to Git.

## 3. Verify the deployment

1. Visit `https://YOUR-DOMAIN/api/config`. Confirm the JSON reports `authConfigured: true`.
2. Visit `https://YOUR-DOMAIN/api/health` and review the result.
3. In a private browser window, create a test account, sign in, reload, confirm the session persists, then sign out and confirm the session is invalidated.
4. Delete the test account through an appropriately secured administrative/database workflow if required by your test plan.

If `authConfigured` is false, verify the production D1 binding name is exactly `JT_AUTH_DB`, then redeploy.

## Production hardening still required

Before public launch, add rate limiting for sign-in/sign-up, email verification, password reset, abuse monitoring, and a tested account recovery process. The current functional auth tests use an in-memory D1 mock and do not prove production deployment correctness.
