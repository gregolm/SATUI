using System.IO;
using System.Text.Json;
using SATUI.Models;
using SATUI.Services;

namespace SATUI.Tests.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _settingsFile;
    private readonly SettingsService _sut;

    public SettingsServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"SATUI_Tests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        _settingsFile = Path.Combine(_tempDir, "settings.json");
        _sut = new SettingsService(_settingsFile);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void Load_WhenFileDoesNotExist_ReturnsDefaultSettings()
    {
        var settings = _sut.Load();

        // Default URL is empty — triggers first-run settings dialog on startup
        settings.ShouldNotBeNull();
        settings.Url.ShouldBeEmpty();
    }

    [Fact]
    public void Save_ThenLoad_RoundTripsSettingsCorrectly()
    {
        var original = new AppSettings { Url = "https://www.test.com" };

        _sut.Save(original);
        var loaded = _sut.Load();

        loaded.Url.ShouldBe(original.Url);
    }

    [Fact]
    public void Save_CreatesFileAndDirectory()
    {
        var settings = new AppSettings { Url = "https://example.com" };

        _sut.Save(settings);

        File.Exists(_settingsFile).ShouldBeTrue();
    }

    [Fact]
    public void Save_WritesValidJson()
    {
        var settings = new AppSettings { Url = "https://example.com" };

        _sut.Save(settings);

        var json = File.ReadAllText(_settingsFile);
        Should.NotThrow(() => JsonSerializer.Deserialize<AppSettings>(json));
    }

    [Fact]
    public void Load_WhenFileContainsCorruptJson_ReturnsDefaultSettings()
    {
        File.WriteAllText(_settingsFile, "{ this is not valid json }}}");

        var settings = _sut.Load();

        // Default URL is empty — triggers first-run settings dialog on startup
        settings.ShouldNotBeNull();
        settings.Url.ShouldBeEmpty();
    }

    [Fact]
    public void Load_WhenFileContainsEmptyObject_ReturnsDefaultSettings()
    {
        File.WriteAllText(_settingsFile, "{}");

        var settings = _sut.Load();

        settings.ShouldNotBeNull();
    }

    [Fact]
    public void Save_MultipleTimes_LastValueWins()
    {
        _sut.Save(new AppSettings { Url = "https://first.com" });
        _sut.Save(new AppSettings { Url = "https://second.com" });

        var loaded = _sut.Load();

        loaded.Url.ShouldBe("https://second.com");
    }
}
