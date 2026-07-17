---
name: verify
description: Build, launch, and drive the Stringy Blazor WASM app to verify changes at the browser surface.
---

# Verifying Stringy

Blazor WebAssembly, fully client-side. One project: `Stringy/Stringy.csproj`.

## Build & launch

```powershell
dotnet build Stringy/Stringy.csproj --nologo -v q
dotnet run --project Stringy/Stringy.csproj --launch-profile https   # run in background
```

Serves on **http://localhost:5290** (and https://localhost:7050). Wait for
"Application started" in the output, then drive with Playwright MCP tools.

## Gotchas

- Razor/C# changes need a server restart (`dotnet run` rebuilds); files in
  `wwwroot/` (app.js, app.css) are static — a page reload is enough.
- Blazor WASM boots asynchronously: after `browser_navigate`, poll for a
  page element (e.g. a known input id) before interacting.
- Playwright screenshots must save under the repo root; allowed root is
  `.playwright-mcp/` (untracked). Pass an absolute filename inside it.
- Clipboard APIs (`stringy.copy` / `stringy.paste`) don't get permission in
  the headless browser — `readText` would hang without the 5s timeout race
  in `wwwroot/js/app.js`; only the error path is testable in automation.
- To independently decode a rendered QR code, inject jsQR from unpkg via a
  script tag in `browser_evaluate`, draw the SVG to a canvas, and decode —
  the dev server sets no CSP so this works.

## Flows worth driving

- Each tool is a route: `/` (transform), `/base64`, `/color`, `/hash`,
  `/encrypt`, `/regex`, `/cron`, `/qr`.
- Theme toggle is the button in `.masthead__tools`; check both themes for
  visual changes (`data-theme` attribute on `<html>`).
