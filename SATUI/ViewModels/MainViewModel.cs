using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SATUI.Models;
using SATUI.Services;

namespace SATUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IConnectivityService _connectivityService;

    [ObservableProperty]
    private string _currentUrl = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    /// <summary>Raised when WebView2 should navigate to a URL.</summary>
    public event Action<string>? NavigationRequested;

    /// <summary>Raised when the Settings dialog should be opened.</summary>
    public event Action? OpenSettingsRequested;

    /// <summary>
    /// Raised when a connection attempt fails. The argument is the URL that was attempted,
    /// so the UI can show it in the error dialog and let the user correct it.
    /// </summary>
    public event Action<string>? ConnectionErrorRequested;

    public MainViewModel(ISettingsService settingsService, IConnectivityService connectivityService)
    {
        _settingsService = settingsService;
        _connectivityService = connectivityService;
    }

    public async Task InitializeAsync()
    {
        var settings = _settingsService.Load();
        CurrentUrl = settings.Url;

        if (string.IsNullOrWhiteSpace(CurrentUrl))
        {
            // No URL configured (e.g. first run) — go straight to Settings
            OpenSettingsRequested?.Invoke();
            return;
        }

        await NavigateAsync();
    }

    [RelayCommand]
    public async Task NavigateAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentUrl))
        {
            OpenSettingsRequested?.Invoke();
            return;
        }

        IsLoading = true;

        var reachable = await _connectivityService.IsReachableAsync(CurrentUrl);

        IsLoading = false;

        if (reachable)
            NavigationRequested?.Invoke(CurrentUrl);
        else
            ConnectionErrorRequested?.Invoke(CurrentUrl);
    }

    [RelayCommand]
    public void OpenSettings()
    {
        OpenSettingsRequested?.Invoke();
    }

    /// <summary>Called by the UI when WebView2 reports a navigation failure.</summary>
    public void OnNavigationFailed(string url, string errorDescription)
    {
        IsLoading = false;
        ConnectionErrorRequested?.Invoke(url);
    }

    public void ApplySettings(AppSettings settings)
    {
        CurrentUrl = settings.Url;
    }
}
