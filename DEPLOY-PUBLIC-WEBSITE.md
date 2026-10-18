# JT Digital World — Public Website

## Target
Visitors open Microsoft Edge and visit a normal HTTPS address such as:
`https://jtdigitalworld.com`

No localhost is required for visitors.

## Architecture
Edge/Chrome/Safari → custom domain + HTTPS → Cloudflare Pages → `public/`
with server-side Pages Functions in `functions/`.

## Deploy
1. Create the Cloudflare Pages project.
2. Connect the Git repository or use Direct Upload.
3. Set the build output directory to `public`.
4. Deploy and test the temporary `*.pages.dev` address.
5. Add the production custom domain in Pages.
6. Configure DNS/nameservers as required by the domain setup.
7. Test the final HTTPS domain in Edge.

## Backend still to connect
- JT Account Service + persistent PostgreSQL
- OAuth providers
- email verification/password recovery
- Stripe billing/webhooks
- app downloads/CDN
- feedback/support API
- monitoring/backups

Do not put API keys, database passwords, Stripe secrets, OAuth secrets, or
private keys in this repository.

## Offers
Public website intentionally does not show pricing. Offers remain inside
the individual JT applications.

## Branding
Agent Jeff / Jeff AI / JTech AI Core.


## Desktop updates
The desktop suite checks `/api/updates/manifest`. Configure the `JT_UPDATE_*` variables in Cloudflare Pages before publishing installers. See `installer-payload/docs/UPDATES.md`.

## v38 exact deployment settings

For the Git-integrated deployment use:
- Production branch: `main`
- Root directory: `Website`
- Build command: `exit 0`
- Build output directory: `public`

See `DEPLOY-PUBLIC-HTTPS-v38.md` for the complete public HTTPS procedure.
