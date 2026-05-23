using Microsoft.Win32;
using System.Windows;

namespace SATUI.Services;

public class WindowsThemeService : IThemeService
{
    private readonly Func<bool> _readIsDarkMode;

    public bool IsDarkMode => _readIsDarkMode();

    public event EventHandler? ThemeChanged;

    // Production constructor: reads from registry and subscribes to system preference changes
    public WindowsThemeService()
        : this(ReadFromRegistry)
    {
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
    }

    // Testable constructor: custom reader, no system event subscription
    internal WindowsThemeService(Func<bool> readIsDarkMode)
    {
        _readIsDarkMode = readIsDarkMode;
    }

    // Exposed for testing to allow simulating a theme change event
    internal void SimulateThemeChange() => ThemeChanged?.Invoke(this, EventArgs.Empty);

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General)
            Application.Current?.Dispatcher.BeginInvoke(() => ThemeChanged?.Invoke(this, EventArgs.Empty));
    }

    private static bool ReadFromRegistry()
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        return key?.GetValue("AppsUseLightTheme") is int i && i == 0;
    }
}
