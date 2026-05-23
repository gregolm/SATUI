# SATUI — Copilot Instructions

SATUI is a Windows 11 WPF/.NET 10 desktop app that embeds web content from a user-configured SAT (Satellite Access Terminal) URL via WebView2.

## Build & Test

```bash
# Build
dotnet build

# Run all tests
dotnet test

# Run a single test by name filter
dotnet test --filter "FullyQualifiedName~MainViewModelTests.NavigateAsync_WhenUnreachable"

# Run all tests in one class
dotnet test --filter "ClassName~MainViewModelTests"

# Publish self-contained single EXE (release target)
dotnet publish SATUI/SATUI.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish/
```

CI runs on `windows-latest` only — the app is Windows-only (WPF + WebView2).

## Architecture

```
SATUI/                    ← WPF application
  App.xaml.cs             ← DI container setup (Microsoft.Extensions.DependencyInjection)
  MainWindow.xaml/.cs     ← View; wires ViewModel events to WebView2 and dialogs
  Models/AppSettings.cs   ← Single model: { Url: string }
  ViewModels/             ← MVVM via CommunityToolkit.Mvvm
  Services/               ← Interfaces + implementations (settings, connectivity)
  Views/                  ← Modal dialogs (Settings, ConnectionError)

SATUI.Tests/              ← xUnit + Moq + FluentAssertions
  ViewModels/             ← Unit tests per ViewModel
  Services/               ← Unit tests per Service
```

### Request flow

1. `App.OnStartup` builds the DI container and resolves `MainWindow`
2. `MainWindow.OnContentRendered` initializes WebView2, then calls `MainViewModel.InitializeAsync()`
3. `InitializeAsync` loads settings → if URL empty, fires `OpenSettingsRequested`; otherwise calls `NavigateAsync()`
4. `NavigateAsync` calls `IConnectivityService.IsReachableAsync()` (HEAD request, 5s timeout), then fires either `NavigationRequested` or `ConnectionErrorRequested`
5. `MainWindow` handles those events directly: sets `WebView.Source` or opens the error dialog

### Dialog flow

Both `SettingsDialog` and `ConnectionErrorDialog` are opened by `MainWindow`, not by ViewModels. `MainWindow` instantiates the dialog's ViewModel directly (not via DI) since dialogs are transient and need constructor arguments. Event handlers are registered and **explicitly deregistered inside the handler** (one-shot pattern) to prevent leaks:

```csharp
void OnSaved(AppSettings settings)
{
    settingsVm.SettingsSaved -= OnSaved;   // deregister self
    settingsVm.Cancelled -= OnCancelled;
    // ...
}
```

## Key Conventions

### CommunityToolkit.Mvvm source generation

All ViewModels are `partial` classes inheriting `ObservableObject`. Use attributes on **private backing fields**:

```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(WindowTitle))]   // notifies another property
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]  // re-evaluates CanExecute
private string _url = string.Empty;
```

To react to a property change without overriding, use the generated partial method hook:

```csharp
partial void OnUrlChanged(string value)
{
    OnPropertyChanged(nameof(UrlValidationError)); // manually notify computed properties
}
```

Commands are generated from `[RelayCommand]`-annotated methods. Async methods produce `AsyncRelayCommand`. `CanExecute` is wired via `CanExecute = nameof(methodName)`.

### ViewModel → View communication via events

ViewModels do not reference the View. They expose typed events; `MainWindow` subscribes:

```csharp
public event Action<string>? NavigationRequested;
public event Action? OpenSettingsRequested;
public event Action<string>? ConnectionErrorRequested;
```

Never add a View reference or `Window.Show*` call to a ViewModel.

### Services are interface-first for testability

Both services have interface + implementation pairs (`ISettingsService`/`SettingsService`, `IConnectivityService`/`ConnectivityService`). `SettingsService` accepts an optional `filePath` constructor parameter so tests can inject a temp path instead of `%APPDATA%\SATUI\settings.json`.

### Connectivity check semantics

`ConnectivityService` treats any HTTP response with status < 500 as reachable (including 3xx, 4xx). Only network-level failures or 5xx responses trigger the connection error dialog.

### Test structure: `CreateSut()` factory

Each test class uses a private `static CreateSut(...)` factory method that constructs mocks and returns a named tuple, keeping individual tests concise:

```csharp
private static (MainViewModel vm, Mock<ISettingsService> s, Mock<IConnectivityService> c)
    CreateSut(string storedUrl = "https://www.example.com", bool isReachable = true) { ... }
```

### Settings persistence

`AppSettings` serializes to JSON at `%APPDATA%\SATUI\settings.json`. If the file is missing or contains invalid JSON, `SettingsService.Load()` silently returns `new AppSettings()` (empty URL), which triggers the first-run Settings dialog.

### WebView2 configuration

Context menus and DevTools are disabled in `MainWindow.OnContentRendered`. Full-screen (F11) saves/restores `WindowStyle`, `ResizeMode`, and `WindowState` manually since WPF has no built-in full-screen mode.
