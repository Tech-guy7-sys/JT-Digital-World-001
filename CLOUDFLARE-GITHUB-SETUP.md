# JT Digital World — GitHub + Cloudflare setup

This guide describes the configuration committed to this repository. It does **not** mean that a Cloudflare Pages project or custom domain has already been connected.

## Current deployment layout

- GitHub repository: `Tech-guy7-sys/jt-digital-world`
- Production branch: `main`
- Static-site build command: `npm run build:website`
- Build output directory: `dist`
- Build script: `scripts/build-site.mjs`
- Wrangler Pages output: `dist`

The build script copies an allowlist of website pages, styles/scripts, app-preview directories and assets into `dist/`. It intentionally does not publish development documentation, desktop source, or workflow files. It also does not publish the root `_redirects` file because its current `/api/*` rewrite is not backed by a Pages Functions directory.

## Current website validation — 2026-10-18

The latest successful website CI run is https://github.com/Tech-guy7-sys/jt-digital-world/actions/runs/39132666373. It syntax-checks the build script, JT Stream Studio script and live-release download script; builds the static site; verifies the main output files; and confirms the JT Stream Studio HTML/CSS/JS files are present in `dist`. It also verifies that all 41 statically referenced studio element IDs exist in the studio HTML.

The working Stage 01 browser studio is at `/jt-apps/jt-stream/studio.html`. Once Cloudflare Pages is connected and deployed, test the live route at `https://<your-project>.pages.dev/jt-apps/jt-stream/studio.html`; do not treat that URL as live until the Pages project has returned its actual hostname.

## 1. Run and verify the build locally

Install Node.js 22 or newer, clone the repository, then run:

```powershell
npm run build:website

test-path dist\index.html
test-path dist\home.html
test-path dist\apps.html
```

The build is static-only. Sign-in, account persistence, update-manifest and other `/api/*` routes must not be treated as live until their server-side Pages Functions and required data bindings are implemented and deployed.

## 2. Connect the existing GitHub repository to Cloudflare Pages

1. Sign in at the Cloudflare dashboard.
2. Open **Workers & Pages** and choose **Create application** / **Pages** / **Connect to Git**.
3. Authorize the Cloudflare GitHub integration for the specific repository `Tech-guy7-sys/jt-digital-world`. Grant only the repository access needed.
4. Select branch `main`.
5. Set the build command to `npm run build:website`.
6. Set the build output directory to `dist`.
7. Leave the root directory at the repository root.
8. Save and start the first deployment.
9. Open the generated `*.pages.dev` preview and verify the homepage, app pages, CSS, images and links.

Cloudflare Pages Git integration can deploy on pushes and provide preview deployments for branches and pull requests. See the official [Git integration docs](https://developers.cloudflare.com/pages/configuration/git-integration/).

## 3. Obtain the domain

The existing brand notes mention `jtdigitalworld.com` as the preferred candidate, but its ownership and availability have **not** been verified. Do not assume it is available.

1. In Cloudflare Dashboard, open **Domain Registration** / **Register domains**.
2. Search for `jtdigitalworld.com` and, if unavailable or unsuitable, search alternatives such as `jtechdigitalworld.com`.
3. Review the exact registration price, renewal price, supported TLD, privacy options and contact details before purchasing.
4. Purchase only after confirming the exact spelling and total cost. Domain registration is a paid action; this repository does not purchase a domain.
5. Complete any registrant email verification Cloudflare requests.

Official instructions: [Register a domain with Cloudflare](https://developers.cloudflare.com/registrar/get-started/register-domain/).

## 4. Attach the purchased domain to Pages

After the first successful Pages deployment:

1. Open the Pages project → **Custom domains** → **Set up a custom domain**.
2. Enter the domain you actually purchased.
3. Follow the dashboard's DNS instructions. If registered through Cloudflare Registrar, its nameservers are managed by Cloudflare.
4. Wait for domain activation and TLS provisioning, then test both the apex domain and any configured `www` hostname.
5. Keep the `*.pages.dev` address available for deployment diagnostics.

Do not manually add guessed DNS records. Use the records shown in the Cloudflare dashboard for the actual project and domain.

## 5. GitHub safeguards

- Keep the repository as the source of truth; review changes before merging to `main`.
- The `.github/workflows/website-ci.yml` workflow validates the static-site build on matching pushes and pull requests.
- Never commit API tokens, database credentials, signing keys or other secrets.
- Prefer Cloudflare's Git integration over storing a deployment token in GitHub Actions for this static website.
- Use GitHub Releases for versioned desktop installers; do not deploy desktop-control functionality to Cloudflare.

## Known backend gap

The repository currently has no `functions/` directory in its root tree. This static build does not implement the API endpoints referenced by some legacy deployment notes. Before advertising account sign-in, password recovery, update manifests, billing or persistent cloud features as operational, implement and test the required server-side endpoints and storage bindings separately.
