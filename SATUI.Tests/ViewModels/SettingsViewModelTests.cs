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

        vm.Url.ShouldBe("https://loaded.com");
    }

    [Fact]
    public void IsValid_WhenUrlIsValidHttps_ReturnsTrue()
    {
        var (vm, _) = CreateSut();
        vm.Url = "https://valid.example.com";

        vm.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WhenUrlIsValidHttp_ReturnsTrue()
    {
        var (vm, _) = CreateSut();
        vm.Url = "http://valid.example.com";

        vm.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WhenUrlIsEmpty_ReturnsFalse()
    {
        var (vm, _) = CreateSut();
        vm.Url = string.Empty;

        vm.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WhenUrlIsWhitespace_ReturnsFalse()
    {
        var (vm, _) = CreateSut();
        vm.Url = "   ";

        vm.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WhenUrlIsMalformed_ReturnsFalse()
    {
        var (vm, _) = CreateSut();
        vm.Url = "@#$%!";  // garbage characters — invalid as host or IP

        vm.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WhenBareIPv4_ReturnsTrue()
    {
        var (vm, _) = CreateSut();
        vm.Url = "192.168.1.100";

        vm.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WhenBareHostname_ReturnsTrue()
    {
        var (vm, _) = CreateSut();
        vm.Url = "sat-terminal";

        vm.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WhenUrlHasNonHttpScheme_ReturnsFalse()
    {
        var (vm, _) = CreateSut();
        vm.Url = "ftp://example.com";

        vm.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void SaveCommand_CanExecute_WhenValid()
    {
        var (vm, _) = CreateSut();
        vm.Url = "https://valid.example.com";

        vm.SaveCommand.CanExecute(null).ShouldBeTrue();
    }

    [Fact]
    public void SaveCommand_CannotExecute_WhenInvalid()
    {
        var (vm, _) = CreateSut();
        vm.Url = string.Empty;

        vm.SaveCommand.CanExecute(null).ShouldBeFalse();
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

        savedSettings.ShouldNotBeNull();
        savedSettings!.Url.ShouldBe("https://new.example.com");
    }

    [Fact]
    public void CancelCommand_WhenAllowCancel_FiresCancelledEvent_WithoutSaving()
    {
        var (vm, settingsMock) = CreateSut();
        bool cancelled = false;
        vm.Cancelled += () => cancelled = true;

        vm.CancelCommand.Execute(null);

        cancelled.ShouldBeTrue();
        settingsMock.Verify(s => s.Save(It.IsAny<AppSettings>()), Times.Never);
    }

    [Fact]
    public void CancelCommand_WhenDisallowCancel_FiresExitRequestedNotCancelled()
    {
        var settingsMock = new Mock<ISettingsService>();
        settingsMock.Setup(s => s.Load()).Returns(new AppSettings());
        var vm = new SettingsViewModel(settingsMock.Object, allowCancel: false);
        bool exitFired = false;
        bool cancelFired = false;
        vm.ExitRequested += () => exitFired = true;
        vm.Cancelled += () => cancelFired = true;

        vm.CancelCommand.Execute(null);

        exitFired.ShouldBeTrue();
        cancelFired.ShouldBeFalse();
    }

    [Fact]
    public void AllowCancel_DefaultsToTrue()
    {
        var (vm, _) = CreateSut();

        vm.AllowCancel.ShouldBeTrue();
    }

    [Fact]
    public void AllowCancel_WhenPassedFalse_IsFalse()
    {
        var settingsMock = new Mock<ISettingsService>();
        settingsMock.Setup(s => s.Load()).Returns(new AppSettings());
        var vm = new SettingsViewModel(settingsMock.Object, allowCancel: false);

        vm.AllowCancel.ShouldBeFalse();
    }

    [Fact]
    public void SecondaryButtonText_WhenAllowCancel_ReturnsCancel()
    {
        var (vm, _) = CreateSut();

        vm.SecondaryButtonText.ShouldBe("Cancel");
    }

    [Fact]
    public void SecondaryButtonText_WhenDisallowCancel_ReturnsExitApplication()
    {
        var settingsMock = new Mock<ISettingsService>();
        settingsMock.Setup(s => s.Load()).Returns(new AppSettings());
        var vm = new SettingsViewModel(settingsMock.Object, allowCancel: false);

        vm.SecondaryButtonText.ShouldBe("Exit Application");
    }

    [Fact]
    public void Save_PreservesLicenseAcceptedFromCurrentSettings()
    {
        var settingsMock = new Mock<ISettingsService>();
        settingsMock.Setup(s => s.Load()).Returns(
            new AppSettings { Url = "https://old.com", LicenseAccepted = true });
        var vm = new SettingsViewModel(settingsMock.Object);
        vm.Url = "https://new.com";

        vm.SaveCommand.Execute(null);

        settingsMock.Verify(
            s => s.Save(It.Is<AppSettings>(a => a.LicenseAccepted == true && a.Url == "https://new.com")),
            Times.Once);
    }

    [Fact]
    public void UrlValidationError_WhenEmpty_ReturnsMessage()
    {
        var (vm, _) = CreateSut();
        vm.Url = string.Empty;

        vm.UrlValidationError.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void UrlValidationError_WhenValid_ReturnsNull()
    {
        var (vm, _) = CreateSut();
        vm.Url = "https://valid.com";

        vm.UrlValidationError.ShouldBeNull();
    }

    [Fact]
    public void UrlHint_WhenBareIP_ReturnsHintText()
    {
        var (vm, _) = CreateSut();
        vm.Url = "192.168.1.100";

        vm.UrlHint.ShouldNotBeNull();
        vm.HasUrlHint.ShouldBeTrue();
    }

    [Fact]
    public void UrlHint_WhenFullHttpsUrl_ReturnsNull()
    {
        var (vm, _) = CreateSut();
        vm.Url = "https://192.168.1.100";

        vm.UrlHint.ShouldBeNull();
        vm.HasUrlHint.ShouldBeFalse();
    }

    [Fact]
    public void ChangingUrl_RaisesPropertyChangedForHintProperties()
    {
        var (vm, _) = CreateSut();
        var changedProps = new List<string?>();
        vm.PropertyChanged += (_, e) => changedProps.Add(e.PropertyName);

        vm.Url = "192.168.1.100";

        changedProps.ShouldContain(nameof(vm.UrlHint));
        changedProps.ShouldContain(nameof(vm.HasUrlHint));
    }
}
