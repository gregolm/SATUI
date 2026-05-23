using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System.Text.Json;

namespace SATUI.UITests.Infrastructure;

/// <summary>
/// Manages the lifecycle of a SATUI process for UI automation tests.
/// Creates/clears the settings file before launch so each test starts in a known state.
/// </summary>
public sealed class AppHarness : IDisposable
{
    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SATUI");

    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    private readonly Application _app;
    public readonly UIA3Automation Automation;

    /// <param name="initialSettings">Settings to write before launch, or null to delete the settings file (clean first-run state).</param>
    public AppHarness(object? initialSettings = null)
    {
        PrepareSettings(initialSettings);
        Automation = new UIA3Automation();
        _app = Application.Launch(FindAppExe());
    }

    /// <summary>Waits for a top-level window whose title contains <paramref name="titleFragment"/>.</summary>
    public Window WaitForWindowByTitle(string titleFragment, int timeoutMs = 10_000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var windows = _app.GetAllTopLevelWindows(Automation);
                var match = windows.FirstOrDefault(w =>
                    w.Title.Contains(titleFragment, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                    return match;
            }
            catch { /* app may still be initializing */ }
            Thread.Sleep(200);
        }
        throw new TimeoutException(
            $"Window containing '{titleFragment}' not found within {timeoutMs}ms");
    }

    private static string FindAppExe()
    {
        // Walk up from the test binary output directory until we find the solution file
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !dir.GetFiles("*.slnx").Any() && !dir.GetFiles("*.sln").Any())
            dir = dir.Parent;

        if (dir == null)
            throw new FileNotFoundException(
                "Cannot locate solution root from: " + AppContext.BaseDirectory);

        foreach (var config in new[] { "Debug", "Release" })
        {
            var path = Path.Combine(
                dir.FullName, "SATUI", "bin", config, "net10.0-windows", "SATUI.exe");
            if (File.Exists(path))
                return path;
        }

        throw new FileNotFoundException(
            $"SATUI.exe not found under {dir.FullName}. Build the SATUI project first.");
    }

    private static void PrepareSettings(object? settings)
    {
        Directory.CreateDirectory(SettingsDir);
        if (settings == null)
        {
            if (File.Exists(SettingsPath))
                File.Delete(SettingsPath);
        }
        else
        {
            File.WriteAllText(SettingsPath,
                JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    public void Dispose()
    {
        try { _app.Close(); } catch { /* best-effort */ }
        Automation.Dispose();
    }
}
