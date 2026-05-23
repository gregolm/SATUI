using SATUI.ViewModels;

namespace SATUI.Tests.ViewModels;

public class ConnectionErrorViewModelTests
{
    private static ConnectionErrorViewModel CreateSut(string url = "https://sat.local")
        => new(url);

    [Fact]
    public void Constructor_SetsAttemptedUrlAndPrePopulatesUrl()
    {
        var vm = CreateSut("https://sat.local/ui");

        vm.AttemptedUrl.ShouldBe("https://sat.local/ui");
        vm.Url.ShouldBe("https://sat.local/ui");
    }

    [Fact]
    public void IsUrlValid_WhenUrlIsValidHttp_IsTrue()
    {
        var vm = CreateSut();
        vm.Url = "http://sat.local";

        vm.IsUrlValid.ShouldBeTrue();
        vm.UrlValidationError.ShouldBeNull();
    }

    [Fact]
    public void IsUrlValid_WhenUrlIsValidHttps_IsTrue()
    {
        var vm = CreateSut();
        vm.Url = "https://192.168.1.1/ui";

        vm.IsUrlValid.ShouldBeTrue();
        vm.HasUrlValidationError.ShouldBeFalse();
    }

    [Fact]
    public void IsUrlValid_WhenUrlIsEmpty_IsFalse()
    {
        var vm = CreateSut();
        vm.Url = string.Empty;

        vm.IsUrlValid.ShouldBeFalse();
        vm.HasUrlValidationError.ShouldBeTrue();
        vm.UrlValidationError.ShouldNotBeNull();
    }

    [Fact]
    public void IsUrlValid_WhenUrlHasNonHttpScheme_IsFalse()
    {
        var vm = CreateSut();
        vm.Url = "ftp://sat.local";

        vm.IsUrlValid.ShouldBeFalse();
        vm.UrlValidationError.ShouldNotBeNull();
    }

    [Fact]
    public void IsUrlValid_WhenUrlIsNotAValidUri_IsFalse()
    {
        var vm = CreateSut();
        vm.Url = "not a url";

        vm.IsUrlValid.ShouldBeFalse();
        vm.HasUrlValidationError.ShouldBeTrue();
    }

    [Fact]
    public void RetryCommand_WhenUrlIsValid_FiresRetryRequestedWithTrimmedUrl()
    {
        var vm = CreateSut();
        vm.Url = "  https://sat.local/dashboard  ";
        string? receivedUrl = null;
        vm.RetryRequested += url => receivedUrl = url;

        vm.RetryCommand.Execute(null);

        receivedUrl.ShouldBe("https://sat.local/dashboard");
    }

    [Fact]
    public void RetryCommand_WhenUrlIsInvalid_CannotExecute()
    {
        var vm = CreateSut();
        vm.Url = string.Empty;

        vm.RetryCommand.CanExecute(null).ShouldBeFalse();
    }

    [Fact]
    public void RetryCommand_WhenUrlIsValid_CanExecute()
    {
        var vm = CreateSut("https://sat.local");

        vm.RetryCommand.CanExecute(null).ShouldBeTrue();
    }

    [Fact]
    public void CancelCommand_FiresCancelledEvent()
    {
        var vm = CreateSut();
        bool fired = false;
        vm.Cancelled += () => fired = true;

        vm.CancelCommand.Execute(null);

        fired.ShouldBeTrue();
    }

    [Fact]
    public void ChangingUrl_RaisesPropertyChangedForValidationProperties()
    {
        var vm = CreateSut();
        var changedProps = new List<string?>();
        vm.PropertyChanged += (_, e) => changedProps.Add(e.PropertyName);

        vm.Url = "bad url";

        changedProps.ShouldContain(nameof(vm.UrlValidationError));
        changedProps.ShouldContain(nameof(vm.HasUrlValidationError));
        changedProps.ShouldContain(nameof(vm.IsUrlValid));
        changedProps.ShouldContain(nameof(vm.UrlHint));
        changedProps.ShouldContain(nameof(vm.HasUrlHint));
    }

    [Fact]
    public void RetryCommand_WhenUrlChangesFromInvalidToValid_BecomesExecutable()
    {
        var vm = CreateSut();
        vm.Url = string.Empty; // invalid

        vm.RetryCommand.CanExecute(null).ShouldBeFalse();

        vm.Url = "https://sat.local"; // valid

        vm.RetryCommand.CanExecute(null).ShouldBeTrue();
    }

    [Fact]
    public void IsUrlValid_WhenBareIPv4_IsTrue()
    {
        var vm = CreateSut();
        vm.Url = "192.168.1.100";

        vm.IsUrlValid.ShouldBeTrue();
    }

    [Fact]
    public void UrlHint_WhenBareIP_ReturnsHintText()
    {
        var vm = CreateSut();
        vm.Url = "192.168.1.100";

        vm.UrlHint.ShouldNotBeNull();
        vm.HasUrlHint.ShouldBeTrue();
    }

    [Fact]
    public void UrlHint_WhenFullHttpsUrl_ReturnsNull()
    {
        var vm = CreateSut();
        vm.Url = "https://sat.local";

        vm.UrlHint.ShouldBeNull();
        vm.HasUrlHint.ShouldBeFalse();
    }
}