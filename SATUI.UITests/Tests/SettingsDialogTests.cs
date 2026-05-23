using FlaUI.Core.AutomationElements;
using SATUI.UITests.Infrastructure;

namespace SATUI.UITests.Tests;

public class SettingsDialogTests : IDisposable
{
    private AppHarness _app = null!;

    public void Dispose()
    {
        _app?.Dispose();
    }

    [Fact]
    public void SettingsDialog_IsShown_WhenNoUrlConfigured()
    {
        // Arrange: license accepted but no URL configured
        var settings = new { Url = "", LicenseAccepted = true };
        _app = new AppHarness(initialSettings: settings);

        // Act: wait for the settings dialog
        var settingsDialog = _app.WaitForWindowByTitle("Settings");

        // Assert: settings dialog is shown
        settingsDialog.ShouldNotBeNull();
        settingsDialog.Title.ShouldBe("Settings");
    }

    [Fact]
    public void SettingsDialog_ShowsExitButton_WhenCancelNotAllowed()
    {
        // Arrange: license accepted but no URL configured (cancel not allowed at first run)
        var settings = new { Url = "", LicenseAccepted = true };
        _app = new AppHarness(initialSettings: settings);
        var settingsDialog = _app.WaitForWindowByTitle("Settings");

        // Act: find the cancel/exit button (should say "Exit Application" when cancel not allowed)
        var cancelButton = settingsDialog.FindFirstDescendant(
            cf => cf.ByAutomationId("CancelButton"))?.AsButton();
        cancelButton.ShouldNotBeNull();

        // Assert: button text should indicate exit, not cancel
        var buttonText = cancelButton.Name;
        buttonText.ShouldContain("Exit");
    }

    [Fact]
    public void SettingsDialog_ShowsValidationError_WhenSavingBlankUrl()
    {
        // Arrange: settings dialog open with blank URL
        var settings = new { Url = "", LicenseAccepted = true };
        _app = new AppHarness(initialSettings: settings);
        var settingsDialog = _app.WaitForWindowByTitle("Settings");

        // Act: try to save without entering a URL
        var saveButton = settingsDialog.FindFirstDescendant(
            cf => cf.ByAutomationId("SaveButton"))?.AsButton();
        saveButton.ShouldNotBeNull();

        // The Save button should be disabled if URL is empty, but let's verify the validation error is shown
        var urlTextBox = settingsDialog.FindFirstDescendant(
            cf => cf.ByAutomationId("UrlTextBox"))?.AsTextBox();
        urlTextBox.ShouldNotBeNull();
        urlTextBox.Text.ShouldBeEmpty();

        // Assert: validation error should be visible
        var validationError = settingsDialog.FindFirstDescendant(
            cf => cf.ByAutomationId("ValidationError"));
        validationError.ShouldNotBeNull("Validation error not found");
    }

    [Fact]
    public void SettingsDialog_ClosesAndShowsMainWindow_WhenValidUrlSaved()
    {
        // Arrange: settings dialog with local test server
        var settings = new { Url = "", LicenseAccepted = true };
        using var server = new LocalHttpServer();
        _app = new AppHarness(initialSettings: settings);
        var settingsDialog = _app.WaitForWindowByTitle("Settings");

        // Act: enter a valid URL (using local test server) and save
        var urlTextBox = settingsDialog.FindFirstDescendant(
            cf => cf.ByAutomationId("UrlTextBox"))?.AsTextBox();
        urlTextBox.ShouldNotBeNull();
        urlTextBox.Text = server.HostAndPort;

        var saveButton = settingsDialog.FindFirstDescendant(
            cf => cf.ByAutomationId("SaveButton"))?.AsButton();
        saveButton.ShouldNotBeNull();
        saveButton.Invoke();

        // Assert: main window appears with the URL in the header
        var mainWindow = _app.WaitForWindowByTitle("SATUI");
        mainWindow.ShouldNotBeNull();
    }
}
