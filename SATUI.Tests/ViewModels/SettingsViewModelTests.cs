using FluentAssertions;
using Moq;
using SATUI.Models;
using SATUI.Services;
using SATUI.ViewModels;

namespace SATUI.Tests.ViewModels;

public class SettingsViewModelTests
{
    private static (SettingsViewModel vm, Mock<ISettingsService> settingsMock)
        CreateSut(string storedUrl = "https://www.example.com")
    {
        var settingsMock = new Mock<ISettingsService>();
        settingsMock.Setup(s => s.Load()).Returns(new AppSettings { Url = storedUrl });
        var vm = new SettingsViewModel(settingsMock.Object);
        return (vm, settingsMock);
    }

    [Fact]
    public void Constructor_LoadsUrlFromSettings()
    {
        var (vm, _) = CreateSut("https://loaded.com");

        vm.Url.Should().Be("https://loaded.com");
    }

    [Fact]
    public void IsValid_WhenUrlIsValidHttps_ReturnsTrue()
    {
        var (vm, _) = CreateSut();
        vm.Url = "https://valid.example.com";

        vm.IsValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WhenUrlIsValidHttp_ReturnsTrue()
    {
        var (vm, _) = CreateSut();
        vm.Url = "http://valid.example.com";

        vm.IsValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WhenUrlIsEmpty_ReturnsFalse()
    {
        var (vm, _) = CreateSut();
        vm.Url = string.Empty;

        vm.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenUrlIsWhitespace_ReturnsFalse()
    {
        var (vm, _) = CreateSut();
        vm.Url = "   ";

        vm.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenUrlIsMalformed_ReturnsFalse()
    {
        var (vm, _) = CreateSut();
        vm.Url = "not-a-url";

        vm.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenUrlHasNonHttpScheme_ReturnsFalse()
    {
        var (vm, _) = CreateSut();
        vm.Url = "ftp://example.com";

        vm.IsValid.Should().BeFalse();
    }

    [Fact]
    public void SaveCommand_CanExecute_WhenValid()
    {
        var (vm, _) = CreateSut();
        vm.Url = "https://valid.example.com";

        vm.SaveCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void SaveCommand_CannotExecute_WhenInvalid()
    {
        var (vm, _) = CreateSut();
        vm.Url = string.Empty;

        vm.SaveCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void SaveCommand_PersistsSettings()
    {
        var (vm, settingsMock) = CreateSut();
        vm.Url = "https://new.example.com";

        vm.SaveCommand.Execute(null);

        settingsMock.Verify(
            s => s.Save(It.Is<AppSettings>(a => a.Url == "https://new.example.com")),
            Times.Once);
    }

    [Fact]
    public void SaveCommand_FiresSettingsSavedEvent()
    {
        var (vm, _) = CreateSut();
        vm.Url = "https://new.example.com";
        AppSettings? savedSettings = null;
        vm.SettingsSaved += s => savedSettings = s;

        vm.SaveCommand.Execute(null);

        savedSettings.Should().NotBeNull();
        savedSettings!.Url.Should().Be("https://new.example.com");
    }

    [Fact]
    public void CancelCommand_FiresCancelledEvent_WithoutSaving()
    {
        var (vm, settingsMock) = CreateSut();
        bool cancelled = false;
        vm.Cancelled += () => cancelled = true;

        vm.CancelCommand.Execute(null);

        cancelled.Should().BeTrue();
        settingsMock.Verify(s => s.Save(It.IsAny<AppSettings>()), Times.Never);
    }

    [Fact]
    public void UrlValidationError_WhenEmpty_ReturnsMessage()
    {
        var (vm, _) = CreateSut();
        vm.Url = string.Empty;

        vm.UrlValidationError.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void UrlValidationError_WhenValid_ReturnsNull()
    {
        var (vm, _) = CreateSut();
        vm.Url = "https://valid.com";

        vm.UrlValidationError.Should().BeNull();
    }
}
