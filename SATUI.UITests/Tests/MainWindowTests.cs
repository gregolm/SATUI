using FlaUI.Core.AutomationElements;
using SATUI.UITests.Infrastructure;

namespace SATUI.UITests.Tests;

public class MainWindowTests : IDisposable
{
    private AppHarness _app = null!;
    private LocalHttpServer _server = null!;

    public void Dispose()
    {
        _server?.Dispose();
        _app?.Dispose();
    }

    [Fact]
    public void MainWindow_ShowsUrlInHeader()
    {
        // Arrange: license accepted, URL configured and reachable
        _server = new LocalHttpServer();
        var settings = new { Url = _server.HostAndPort, LicenseAccepted = true };
        _app = new AppHarness(initialSettings: settings);

        // Act: wait for main window to appear
        var mainWindow = _app.WaitForWindowByTitle("SATUI");

        // Assert: title shows the URL
        mainWindow.Title.ShouldContain(_server.HostAndPort);
    }

    [Fact]
    public void ClickingUrlInHeader_OpensSettingsDialog()
    {
        // Arrange: main window visible
        _server = new LocalHttpServer();
        var settings = new { Url = _server.HostAndPort, LicenseAccepted = true };
        _app = new AppHarness(initialSettings: settings);
        var mainWindow = _app.WaitForWindowByTitle("SATUI");

        // Act: click the URL button in the header
        var urlButton = mainWindow.FindFirstDescendant(
            cf => cf.ByAutomationId("UrlButton"))?.AsButton();
        urlButton.ShouldNotBeNull("URL button not found in header");
        urlButton.Invoke();

        // Assert: settings dialog appears
        var settingsDialog = _app.WaitForWindowByTitle("Settings", timeoutMs: 5_000);
        settingsDialog.ShouldNotBeNull();
    }

    [Fact]
    public void ClickingGearIcon_OpensSettingsDialog()
    {
        // Arrange: main window visible
        _server = new LocalHttpServer();
        var settings = new { Url = _server.HostAndPort, LicenseAccepted = true };
        _app = new AppHarness(initialSettings: settings);
        var mainWindow = _app.WaitForWindowByTitle("SATUI");

        // Act: click the gear icon button
        var gearButton = mainWindow.FindFirstDescendant(
            cf => cf.ByAutomationId("GearButton"))?.AsButton();
        gearButton.ShouldNotBeNull("Gear button not found in header");
        gearButton.Invoke();

        // Assert: settings dialog appears
        var settingsDialog = _app.WaitForWindowByTitle("Settings", timeoutMs: 5_000);
        settingsDialog.ShouldNotBeNull();
    }

    [Fact]
    public void ReloadButton_IsPresentInHeader()
    {
        // Arrange: main window visible
        _server = new LocalHttpServer();
        var settings = new { Url = _server.HostAndPort, LicenseAccepted = true };
        _app = new AppHarness(initialSettings: settings);
        var mainWindow = _app.WaitForWindowByTitle("SATUI");

        // Act: find the reload button
        var reloadButton = mainWindow.FindFirstDescendant(
            cf => cf.ByAutomationId("ReloadButton"))?.AsButton();

        // Assert: reload button is present
        reloadButton.ShouldNotBeNull("Reload button not found in header");
    }

    [Fact]
    public void HeaderButtons_AllPresent()
    {
        // Arrange: main window visible
        _server = new LocalHttpServer();
        var settings = new { Url = _server.HostAndPort, LicenseAccepted = true };
        _app = new AppHarness(initialSettings: settings);
        var mainWindow = _app.WaitForWindowByTitle("SATUI");

        // Act: find all header buttons
        var urlButton = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("UrlButton"))?.AsButton();
        var reloadButton = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("ReloadButton"))?.AsButton();
        var gearButton = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("GearButton"))?.AsButton();

        // Assert: all buttons present
        urlButton.ShouldNotBeNull("URL button missing");
        reloadButton.ShouldNotBeNull("Reload button missing");
        gearButton.ShouldNotBeNull("Gear button missing");
    }
}
