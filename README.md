# Stringy

A fast, private **developer string toolkit** — text transforms, Base64, colour conversion, hashing, AES encryption, regex testing and a cron builder, all in one place.

Stringy is a **Blazor WebAssembly standalone** app. Everything runs client-side in your browser: there is no backend, no analytics, no ads, and no network calls. Load it once and it works offline.

> Ported from a Next.js app ("TextCraft") to a front-end-only .NET WebAssembly app.

## Features

| Tool | What it does |
|------|--------------|
| **Transform** | Convert text between UPPERCASE, lowercase, camelCase, PascalCase, kebab-case and permalink/slug. Updates live as you type. |
| **Base64** | Encode/decode Base64. UTF-8 aware, so emoji and accented characters round-trip cleanly. |
| **Color** | Convert between RGB/RGBA and HEX (with alpha), auto-detecting the input format. Includes an HSL colour wheel picker. |
| **Hash** | Compute SHA-256, SHA-512 and MD5 digests of your input. |
| **Encrypt** | AES-256-CBC encryption/decryption with a password-derived key (PBKDF2, HMAC-SHA1, 10,000 iterations). Output is `Base64(salt + iv + ciphertext)`, interoperable with the CryptoJS default format. |
| **Regex** | Test a string against a regular expression live, with match counts. Ships with common ready-made patterns and a token cheatsheet. Flags `i`, `m`, `s` supported. |
| **Cron** | Build and read 5-field cron schedules — get a plain-English description live (e.g. `30 9 * * 1-5` → "At 09:30, only on weekdays"), with presets, a per-field legend and a syntax cheatsheet. |

Plus a light/dark theme toggle that remembers your choice and respects your OS preference.

## Privacy

Stringy is built to be trustworthy with sensitive input:

- **100% client-side** — all processing happens in your browser via WebAssembly.
- **No tracking** — no analytics, no telemetry, no cookies.
- **No ads.**
- **No external requests** — no CDNs, no web fonts, no third-party scripts. `referrer` is set to `no-referrer`.

Your text never leaves your machine.

## Tech stack

- **.NET 10** / **Blazor WebAssembly** (standalone)
- **BouncyCastle.Cryptography** — pure-managed MD5 and AES so crypto works inside the browser sandbox
- Custom CSS design system (no UI framework, no external fonts)

## Getting started

### Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) or later
- (Optional, recommended for smaller/faster publishes) the WebAssembly tools workload:
  ```bash
  dotnet workload install wasm-tools
  ```

### Run locally

```bash
dotnet run --project Stringy/Stringy.csproj
```

Then open the URL printed in the console (default `http://localhost:5290`).

### Build

```bash
dotnet build
```

### Publish (static site)

```bash
dotnet publish Stringy/Stringy.csproj -c Release -o publish
```

The deployable static site is produced in `publish/wwwroot`. Because it's a single-page app, host it with a **SPA fallback** so unknown routes serve `index.html` (e.g. GitHub Pages with a `404.html` copy, Netlify/Vercel rewrites, or `try_files` in nginx).

## Project structure

```
Stringy/
├── Core/                 # Pure C# logic (no UI) — fully unit-testable
│   ├── StringTransformer.cs
│   ├── Base64Tool.cs
│   ├── ColorTool.cs
│   ├── Hasher.cs
│   ├── CryptoTool.cs
│   ├── RegexTester.cs
│   ├── RegexHelpers.cs
│   └── CronTool.cs
├── Components/           # Reusable Razor components
│   ├── ThemeToggle.razor
│   ├── CopyButton.razor
│   ├── ColorWheel.razor
│   └── ErrorAlert.razor
├── Layout/
│   └── MainLayout.razor  # Header, tab navigation, footer
├── Pages/                # One routable page per tool
│   ├── Home.razor        # /         Transform
│   ├── Base64.razor      # /base64
│   ├── ColorConverter.razor  # /color
│   ├── Hashing.razor     # /hash
│   ├── Encryption.razor  # /encrypt
│   ├── RegexValidator.razor  # /regex
│   ├── Cron.razor        # /cron
│   └── NotFound.razor
├── Services/
│   ├── ThemeService.cs      # theme state + localStorage
│   └── ClipboardService.cs  # copy-to-clipboard interop
└── wwwroot/
    ├── index.html
    ├── favicon.svg
    ├── css/app.css       # design system
    └── js/app.js         # minimal interop (theme, clipboard) — no network calls
```

## Notes on browser crypto

The .NET framework's `System.Security.Cryptography.MD5` and `Aes.Create()` are unsupported on the `browser` platform and throw at runtime. Stringy therefore uses **BouncyCastle** (pure-managed, trim-safe) for MD5 and AES. `SHA256`/`SHA512`, `PBKDF2` and the cryptographic RNG are browser-supported and use the framework directly.

## License

© Miha Jakovac. All rights reserved.
