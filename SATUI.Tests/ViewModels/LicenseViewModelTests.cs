using SATUI.ViewModels;

namespace SATUI.Tests.ViewModels;

public class LicenseViewModelTests
{
    [Fact]
    public void LicenseText_IsNotNullOrEmpty()
    {
        var vm = new LicenseViewModel();

        vm.LicenseText.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void LicenseText_ContainsExpectedContent()
    {
        var vm = new LicenseViewModel();

        vm.LicenseText.ShouldContain("License Agreement");
        vm.LicenseText.ShouldContain("MIT License");
    }

    [Fact]
    public void AcceptCommand_FiresLicenseAcceptedEvent()
    {
        var vm = new LicenseViewModel();
        bool accepted = false;
        vm.LicenseAccepted += () => accepted = true;

        vm.AcceptCommand.Execute(null);

        accepted.ShouldBeTrue();
    }

    [Fact]
    public void DeclineCommand_FiresLicenseDeclinedEvent()
    {
        var vm = new LicenseViewModel();
        bool declined = false;
        vm.LicenseDeclined += () => declined = true;

        vm.DeclineCommand.Execute(null);

        declined.ShouldBeTrue();
    }

    [Fact]
    public void AcceptCommand_DoesNotFireLicenseDeclinedEvent()
    {
        var vm = new LicenseViewModel();
        bool declined = false;
        vm.LicenseDeclined += () => declined = true;

        vm.AcceptCommand.Execute(null);

        declined.ShouldBeFalse();
    }

    [Fact]
    public void DeclineCommand_DoesNotFireLicenseAcceptedEvent()
    {
        var vm = new LicenseViewModel();
        bool accepted = false;
        vm.LicenseAccepted += () => accepted = true;

        vm.DeclineCommand.Execute(null);

        accepted.ShouldBeFalse();
    }
}
