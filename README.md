# 📡 SATUI — Satellite Access Terminal UI

[![CI](https://github.com/gregolm/SATUI/actions/workflows/ci.yml/badge.svg)](https://github.com/gregolm/SATUI/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/gregolm/SATUI?label=release&color=6C63FF)](https://github.com/gregolm/SATUI/releases/latest)

SATUI is a clean Windows desktop app that puts your Satellite Access Terminal's web interface front and center. Point it at your SAT's URL once and it launches straight into a full-screen-capable, kiosk-style browser window — no browser chrome, no distractions.

## Screenshots

_Coming soon_

## Installation

Download the latest release from the [Releases page](https://github.com/gregolm/SATUI/releases/latest).

| Installer | Description |
|---|---|
| **SATUI-Setup.zip** | Recommended. Extract and run `Install.bat` (requires administrator). Creates Start Menu shortcut and adds SATUI to Programs & Features. |
| **SATUI-\*.msix** | MSIX package. Run the `.msix` file directly if your system is configured to trust the package. |
| **SATUI-win-x64-portable.zip** | No install needed. Extract anywhere and run `SATUI.exe`. Settings are saved to `%APPDATA%\SATUI`. |

### Requirements

- Windows 11 (x64)
- WebView2 Runtime — already included in Windows 11

## Getting Started

**First run:** SATUI will immediately ask you for your SAT's URL. Enter the full address (e.g. `http://192.168.1.1`) and click **Save**. The app connects and loads the interface.

**Change the URL later:** Click the URL shown in the title bar, or click the ⚙️ gear icon at the top right.

**Connection problems:** If SATUI can't reach the SAT, a dialog will appear with steps to try. You can enter a new URL or retry from there.

**Full screen:** Press **F11** to toggle borderless full-screen mode. Standard window controls (maximize, minimize, resize) work as expected.

## Features

- **Embedded Chromium browser** — WebView2 renders the SAT's web UI natively
- **First-run setup** — prompts for URL on first launch, no manual config file needed
- **Smart connection errors** — friendly recovery dialog with retry and URL-change options
- **Clickable header bar** — click the URL or the ⚙️ icon to open settings at any time
- **Full-screen mode** — F11 for a true borderless, distraction-free view
- **Settings persistence** — URL saved automatically to `%APPDATA%\SATUI\settings.json`
- **Windows 11 native feel** — respects light/dark theme, standard window behavior

---

## For Developers

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Build & Test

```bash
git clone https://github.com/gregolm/SATUI.git
cd SATUI
dotnet build
dotnet test SATUI.Tests/SATUI.Tests.csproj
```

### Publish (self-contained)

```bash
dotnet publish SATUI/SATUI.csproj -c Release -r win-x64 --self-contained \
  -p:TrimMode=link -p:EnableTrimAnalyzer=false -o publish/
```

### Architecture

```
SATUI/
├── Models/         — AppSettings { Url, LicenseAccepted }
├── ViewModels/     — MainViewModel, SettingsViewModel, LicenseViewModel, ConnectionErrorViewModel
├── Views/          — SettingsDialog, LicenseDialog, ConnectionErrorDialog
├── Services/       — ISettingsService, IConnectivityService, IThemeService
└── Resources/      — satellite.ico

SATUI.Tests/        — 110 unit tests (xUnit + Moq + Shouldly)
SATUI.UITests/      — FlaUI integration tests (requires display)
```

- **MVVM** via `CommunityToolkit.Mvvm` (source-generated commands and properties)
- **DI** via `Microsoft.Extensions.DependencyInjection`
- **ViewModel → View communication** via typed events (no View references in ViewModels)

### CI / CD

| Trigger | Action |
|---|---|
| Push / PR → `main` | Build + unit tests + code coverage |
| Tag `v*.*.*` | Build, publish, package MSIX + installer ZIP + portable ZIP, create GitHub Release |

## License

MIT
