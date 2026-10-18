# JT Digital World — Exact Public HTTPS Deployment (v38)

## What this setup does

This package is prepared so the JT Digital World website can be published at a normal HTTPS address and opened directly in Microsoft Edge. Visitors do **not** need localhost, Node.js, VS Code, or the JT desktop apps installed just to visit the website.

Recommended deployment:

Microsoft Edge → HTTPS custom domain → Cloudflare Pages → `Website/public/`

The website is a static HTML/CSS/JavaScript site with Cloudflare Pages Functions under `Website/functions/`.

## Important

The files are deployment-ready, but a public domain and Cloudflare account still have to be connected. This package cannot create or control a domain/DNS account by itself.

Do not put passwords, API keys, database credentials, Stripe secrets, OAuth secrets, or private keys in GitHub or this website folder.

---

## 1. Create the GitHub repository

Create a new repository for the JT Digital World project and upload the contents of this package.

Recommended repository name:

`jt-digital-world`

Keep the repository private if it contains desktop application source code or other non-public project files.

The repository should contain:

`Website/`

`installer-payload/`

`command-agent/`

and the other project directories.

Production website deployment only publishes `Website/public/` plus the Pages Functions.

---

## 2. Connect the repository to Cloudflare Pages

In Cloudflare:

Workers & Pages → Create application → Pages → Connect to Git

Select the GitHub repository.

Use these exact build settings:

| Setting | Value |
|---|---|
| Production branch | `main` |
| Root directory | `Website` |
| Build command | `exit 0` |
| Build output directory | `public` |
|

No npm build is required for the current static website.

After the first deployment, Cloudflare will provide a temporary address similar to:

`https://jt-digital-world.pages.dev`

Test that address before connecting the custom domain.

---

## 3. Add the production custom domain

In the Cloudflare Pages project:

Custom domains → Set up a domain

Enter the real domain you own, for example:

`jtdigitalworld.com`

Do not use the example above unless that is actually the domain you own.

For an apex domain such as `example.com`, Cloudflare requires the domain to be added as a Cloudflare zone and its nameservers configured for Cloudflare.

For a subdomain such as `www.example.com`, a CNAME can point to the Pages project, but the hostname must still be added through the Pages Custom Domains screen.

---

## 4. HTTPS

Cloudflare provides the public TLS certificate for the custom domain through Universal SSL when the domain is active on Cloudflare.

After the certificate is active, visitors use:

`https://YOUR-DOMAIN`

Do not publish the website using `http://` as the main address.

Recommended SSL/TLS setting:

`Full (strict)` when an origin server is involved.

For the current static Pages deployment, Cloudflare Pages handles the public edge connection.

---

## 5. Make the custom domain the public address

After the custom domain works, optionally redirect the generated Pages address to the custom domain so customers use only the branded address.

Example:

`https://jt-digital-world.pages.dev` → `https://YOUR-DOMAIN`

Use a Cloudflare Bulk Redirect for this rather than changing DNS away from the Pages project.

---

## 6. Automatic website updates

With the Git integration enabled, pushes to the production branch automatically trigger a new deployment.

Recommended workflow:

1. Command Agent prepares a change.
2. Verification scripts run.
3. Change is committed to GitHub.
4. Cloudflare creates a preview deployment for a branch/PR.
5. Review the preview.
6. Merge to `main`.
7. Cloudflare deploys the production website.
8. Verify the HTTPS domain in Edge.

Do not put production secrets in Git commits.

---

## 7. Required final checks in Microsoft Edge

Open the final HTTPS address and verify:

- Home page loads.
- JT hero image loads.
- JT app section loads.
- All 8 app preview links open.
- CSS loads correctly.
- JavaScript interactions work.
- Mobile/responsive layout works.
- No browser address contains `localhost`.
- HTTPS lock/security indicator is present.
- No mixed-content warnings appear.

Also test:

`https://YOUR-DOMAIN/api/health`

It should return the health response from the Cloudflare Function.

---

## 8. What is NOT automatically live yet

The public website deployment does not automatically create the following production services:

- persistent customer accounts
- Google/GitHub/Apple/Discord OAuth
- email verification/password recovery
- Stripe products, prices, checkout, and webhooks
- PostgreSQL production database
- production feedback/support backend
- production file storage/CDN
- signed Windows/macOS installers
- production monitoring and alerting
- production AI provider credentials

Those must be configured separately and securely.

The public website intentionally does not display the app subscription offers; those remain inside the JT applications.

---

## 9. Local Edge test is different from public deployment

`RUN-WEBSITE-IN-EDGE.bat` is only a local test helper. It does not make the website public.

A public deployment is reached through a domain such as:

`https://YOUR-DOMAIN`

No localhost is involved for visitors.
