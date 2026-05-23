using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Windows;
using SATUI.Services;
using SATUI.ViewModels;

namespace SATUI;

public partial class App : Application
{
    private IServiceProvider _services = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        services.AddHttpClient(nameof(ConnectivityService));
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IConnectivityService, ConnectivityService>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();

        _services = services.BuildServiceProvider();

        var mainWindow = _services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}

