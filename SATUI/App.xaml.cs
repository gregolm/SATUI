using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Windows;
using SATUI.Services;
using SATUI.ViewModels;
using SATUI.Views;

namespace SATUI;

public partial class App : Application
{
    private IServiceProvider _services = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        services.AddHttpClient(nameof(ConnectivityService));
        services.AddSingleton<IThemeService, WindowsThemeService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IConnectivityService, ConnectivityService>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();

        _services = services.BuildServiceProvider();

        // Prevent WPF from shutting down when the license dialog closes before MainWindow opens
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var themeService = _services.GetRequiredService<IThemeService>();
        ApplyTheme(themeService.IsDarkMode);
        themeService.ThemeChanged += (_, _) => ApplyTheme(themeService.IsDarkMode);

        var settingsService = _services.GetRequiredService<ISettingsService>();
        if (!EnsureLicenseAccepted(settingsService))
        {
            Shutdown();
            return;
        }

        var mainWindow = _services.GetRequiredService<MainWindow>();
        mainWindow.Closed += (_, _) => Shutdown();
        mainWindow.Show();
    }

    private bool EnsureLicenseAccepted(ISettingsService settingsService)
    {
        var settings = settingsService.Load();
        if (settings.LicenseAccepted)
            return true;

        var licenseVm = new LicenseViewModel();
        var dialog = new LicenseDialog(licenseVm);

        bool accepted = false;
        licenseVm.LicenseAccepted += () => { accepted = true; dialog.Close(); };
        licenseVm.LicenseDeclined += () => dialog.Close();

        dialog.ShowDialog();

        if (accepted)
        {
            settings.LicenseAccepted = true;
            settingsService.Save(settings);
        }

        return accepted;
    }

    private void ApplyTheme(bool isDark)
    {
        var themeUri = new Uri(
            isDark ? "Themes/Dark.xaml" : "Themes/Light.xaml",
            UriKind.Relative);

        var existing = Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source?.OriginalString.Contains("/Themes/") == true);

        if (existing != null)
            Resources.MergedDictionaries.Remove(existing);

        Resources.MergedDictionaries.Insert(0, new ResourceDictionary { Source = themeUri });
    }
}

