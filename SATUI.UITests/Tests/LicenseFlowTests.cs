using FlaUI.Core.AutomationElements;
using SATUI.UITests.Infrastructure;

namespace SATUI.UITests.Tests;

public class LicenseFlowTests : IDisposable
{
    private AppHarness _app = null!;

    public void Dispose()
    {
        _app?.Dispose();
    }

    [Fact]
    public void OnFirstRun_LicenseDialogIsShown()
    {
        // Arrange: start with no settings (clean first-run state)
        _app = new AppHarness(initialSettings: null);

        // Act: the license dialog should appear
        var licenseDialog = _app.WaitForWindowByTitle("License Agreement", timeoutMs: 5_000);

        // Assert: dialog is shown
        licenseDialog.ShouldNotBeNull();
        licenseDialog.Title.ShouldContain("License Agreement");
    }

    [Fact]
    public void AcceptingLicense_ProceedsToSettingsDialog()
    {
        // Arrange: start with no settings
        _app = new AppHarness(initialSettings: null);
        var licenseDialog = _app.WaitForWindowByTitle("License Agreement");

        // Act: click "I Accept" button
        var acceptButton = licenseDialog.FindFirstDescendant(
            cf => cf.ByAutomationId("AcceptButton"))?.AsButton();
        acceptButton.ShouldNotBeNull("Accept button not found");
        acceptButton.Invoke();

        // Assert: settings dialog appears (no URL → goes to settings)
        var settingsDialog = _app.WaitForWindowByTitle("Settings", timeoutMs: 5_000);
        settingsDialog.ShouldNotBeNull();
        settingsDialog.Title.ShouldBe("Settings");
    }

    [Fact]
    public void DecliningLicense_ExitsApplication()
    {
        // Arrange: start with no settings
        _app = new AppHarness(initialSettings: null);
        var licenseDialog = _app.WaitForWindowByTitle("License Agreement");

        // Act: click "Decline" button
        var declineButton = licenseDialog.FindFirstDescendant(
            cf => cf.ByAutomationId("DeclineButton"))?.AsButton();
        declineButton.ShouldNotBeNull("Decline button not found");
        declineButton.Invoke();

        // Assert: the app process should exit
        // Wait a bit for the window to close
        Thread.Sleep(500);
        // If the window closes without another dialog appearing, the test passes
        try
        {
            _app.WaitForWindowByTitle("License Agreement", timeoutMs: 1_000);
            true.ShouldBeFalse("App did not exit after declining license");
        }
        catch (TimeoutException)
        {
            // Expected — the license dialog closed
        }
    }
}
