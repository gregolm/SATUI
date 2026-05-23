# 📡 SATUI — Satellite Access Terminal User Interface

A Windows 11 desktop application built with .NET 10 (WPF) that embeds web content from a configured satellite access terminal URL, providing a clean kiosk-style interface for satellite radio communication systems.

## Features

- **Embedded browser** — Chromium-based WebView2 renders the terminal's web UI
- **Auto-configure on first run** — opens Settings dialog immediately if no URL is configured
- **Connection error recovery** — friendly dialog prompts to verify the SAT is powered on, with option to change the URL and retry
- **Settings persistence** — URL saved to `%APPDATA%\SATUI\settings.json`
- **Full-screen support** — F11 toggles true borderless full-screen
- **Standard window behavior** — maximize, minimize, resize all work as expected

## Screenshots

_Coming soon_

## Requirements

- Windows 11 (x64)
- [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) (included in Windows 11)

## Building from Source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Build

```bash
git clone https://github.com/gregolm/SATUI.git
cd SATUI
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Publish (self-contained single EXE)

```bash
dotnet publish SATUI/SATUI.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish/
```

## Architecture

```
SATUI/
├── Models/         — AppSettings (URL config)
├── ViewModels/     — MainViewModel, SettingsViewModel, ConnectionErrorViewModel
├── Views/          — SettingsDialog, ConnectionErrorDialog
├── Services/       — ISettingsService, IConnectivityService
└── Resources/      — satellite.ico (multi-size, 16/32/48/256px)

SATUI.Tests/
├── ViewModels/     — MainViewModelTests, SettingsViewModelTests, ConnectionErrorViewModelTests
└── Services/       — SettingsServiceTests, ConnectivityServiceTests
```

- **MVVM** via `CommunityToolkit.Mvvm` with constructor-injected interfaces
- **DI** via `Microsoft.Extensions.DependencyInjection`
- **Tests** via xUnit + Moq + FluentAssertions
- **Coverage** via Coverlet

## CI / CD

| Trigger | Action |
|---|---|
| Push / PR → main | Build + test + coverage report |
| Tag `v*.*.*` | Build, publish, create GitHub Release with EXE |

## License

MIT
