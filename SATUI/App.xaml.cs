using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Windows;
using System.IO;
using SATUI.Services;
using SATUI.ViewModels;
using SATUI.Views;

namespace SATUI;

public partial class App : Application
{
    private IServiceProvider _services = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SATUI", "startup_error.log");

        try
        {
            // Log startup attempt
            AppendLog(logPath, "Application startup called");
            
            base.OnStartup(e);
            AppendLog(logPath, "Base OnStartup completed");

            var services = new ServiceCollection();

            services.AddHttpClient(nameof(ConnectivityService));
            services.AddSingleton<IThemeService, WindowsThemeService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IConnectivityService, ConnectivityService>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindow>();

            _services = services.BuildServiceProvider();
            AppendLog(logPath, "DI container built");

            // Prevent WPF from shutting down when the license dialog closes before MainWindow opens
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var themeService = _services.GetRequiredService<IThemeService>();
            ApplyTheme(themeService.IsDarkMode);
            themeService.ThemeChanged += (_, _) => ApplyTheme(themeService.IsDarkMode);
            AppendLog(logPath, "Theme applied");

            var settingsService = _services.GetRequiredService<ISettingsService>();
            if (!EnsureLicenseAccepted(settingsService))
            {
                AppendLog(logPath, "License not accepted, shutting down");
                Shutdown();
                return;
            }

            var mainWindow = _services.GetRequiredService<MainWindow>();
            mainWindow.Closed += (_, _) => Shutdown();
            mainWindow.Show();
            AppendLog(logPath, "MainWindow shown successfully");
        }
        catch (Exception ex)
        {
            AppendLog(logPath, $"EXCEPTION: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            
            try
            {
                MessageBox.Show(
                    $"Application startup failed: {ex.Message}",
                    "SATUI Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch { /* MessageBox failed, ignore */ }

            Shutdown(1);
        }
    }

    private static void AppendLog(string logPath, string message)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            File.AppendAllText(logPath, $"[{DateTime.Now:O}] {message}\n");
        }
        catch { /* Ignore logging errors */ }
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

