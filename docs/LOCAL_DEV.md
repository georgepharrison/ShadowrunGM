# Local Dev & Mobile Testing (Docker + Nginx)

This guide shows how to build and run the Blazor WebAssembly UI in Docker behind Nginx, test it on your phone over LAN, and sanity-check PWA behavior.

---

## Prereqs

- Docker Desktop (Windows/macOS) or Docker Engine (Linux)
- .NET 9 SDK (optional — only needed if you build locally without Docker)
- A terminal and a pulse 😄

---

## Build & Run

From the **solution root**:

```
# Build
docker build -t shadowrun-ui:dev -f src/UI/Dockerfile .

# Run on port 8080
docker run --rm -it -p 8080:80 --name shadowrun-ui shadowrun-ui:dev
```

Open: http://localhost:8080

> The Dockerfile publishes the WASM app and serves `/wwwroot` from Nginx using a template that sets cache headers right for a PWA.

---

## LAN / Phone Testing

1. Find your machine’s LAN IP:
   - Windows: `ipconfig` → e.g. `192.168.1.42`
   - macOS/Linux: `ifconfig` / `ip addr`
2. Make sure your firewall allows inbound **TCP 8080** (or the port you used).
3. On your phone (same Wi-Fi), open:  
   `http://<your-lan-ip>:8080`

If you don’t see it:
- Confirm container is running: `docker ps`
- Try a different port mapping: `-p 3000:80`
- Windows + WSL2 users: ensure Docker shares the correct network interface

---

## PWA Install Tips

- **Mobile (Chrome/Edge):** Open the site → browser menu → **Add to Home screen**.  
  On Android, Chrome may keep it in a Chrome shell if using plain `http` and/or an IP. Full “standalone app” behavior is more consistent with **HTTPS** and a **domain**. (See HTTPS below.)
- **Desktop (Chrome/Edge):** Address bar → **Install app**.

### What gets used
- `manifest-light.webmanifest` / `manifest-dark.webmanifest` (we swap based on system theme)
- `icon-192.png`, `icon-512.png`, and `favicon.png`
- `service-worker.published.js` in Release/Publish builds

---

## HTTPS (optional but recommended)

For best PWA install UX (especially on Android), serve over HTTPS with a real hostname.

**Quick local option with `mkcert`**:
1. Install mkcert and generate a cert:
   ```
   mkcert -install
   mkcert localhost 127.0.0.1
   ```
   Produces `localhost+1.pem` and `localhost+1-key.pem`.
2. Add an Nginx TLS server block (or run a sidecar like `nginx:alpine` with TLS) that points to the files above.
3. Access via `https://localhost:8443` (for example).

(If you want a turnkey HTTPS compose later, we can add a `docker-compose.yml` with a TLS-enabled Nginx that reverse-proxies to the static server.)

---

## Cache Busting & Updates

- We already append a `?v=` to MudBlazor CSS/JS in `index.html`.  
  When you bump MudBlazor, also bump that `v=` value to avoid stale caches.
- After publishing a new build, **hard refresh**:
  - Desktop: `Ctrl+F5` (Windows) or `Cmd+Shift+R` (macOS)
  - Phone (Chrome): open tabs → three dots → **Reload** or clear site data

---

## Common Tasks

### Rebuild clean
```
docker build --no-cache -t shadowrun-ui:dev -f src/UI/Dockerfile .
```

### Tail Nginx logs
```
docker exec -it shadowrun-ui sh -c "tail -f /var/log/nginx/access.log /var/log/nginx/error.log"
```

### Stop container
```
docker stop shadowrun-ui
```

---

## Troubleshooting

**“White screen” or stale UI after publish**
- Service worker still serving old assets. Do a hard refresh; if needed, uninstall the PWA, clear site storage, and reinstall.

**PWA doesn’t install as standalone on Android**
- Expected if using `http://` and plain IP.  
  Use HTTPS + a hostname (even `http://` + **hostnames** may behave better than IPs, but HTTPS is king).

**`UI.styles.css` 404 in dev**
- That file is generated when you have scoped CSS in the project. If you don’t, it won’t exist — remove the link or keep it harmlessly missing in dev. In publish with styles, it will appear under `wwwroot`.

**“Fetch event handler is recognized as no-op” warning**
- Harmless in development. In production, ensure `service-worker.published.js` handles install/activate/fetch (your file already does).

**Can’t reach from phone**
- Host firewall blocking inbound port (Windows Defender Firewall → allow Docker or the chosen port).
- Wrong IP (laptops on VPN often have multiple adapters).

---

## Conventional Commits (suggested)

```
# Examples
git add -A
git commit -m "feat(ui): docker + nginx static hosting for WASM"
git commit -m "chore(pwa): manifest split by theme, service worker tweaks"
git commit -m "docs: add local dev & mobile testing guide"
```

---

## Useful Files

- `src/UI/Dockerfile` – multi-stage build (SDK → Nginx static serve)
- `src/UI/nginx.conf.template` – Nginx config (templated)
- `wwwroot/manifest-light.webmanifest` / `manifest-dark.webmanifest`
- `wwwroot/service-worker.published.js` / `service-worker.js`
- `wwwroot/icon-192.png`, `wwwroot/icon-512.png`, `wwwroot/favicon.png`

---

## .dockerignore (recommended)

At the **solution root**:

```
**/bin/
**/obj/
**/.vs/
**/.idea/
**/.vscode/
**/*.user
**/*.suo
**/*.csproj.user
**/*.swp
**/*.DS_Store
artifacts/
node_modules/
```

---

## Next Steps

- Add `docker-compose.yml` for one-command up, optional HTTPS, and future API integration.
- Add a **Health** page or `/status.json` to quickly verify versions and cache status from mobile.
- Wire a CI job (GitHub Actions, Azure DevOps) to build the image on push to `main`.

Happy shipping! 🚀
