using FluentAssertions;
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

        vm.AttemptedUrl.Should().Be("https://sat.local/ui");
        vm.Url.Should().Be("https://sat.local/ui");
    }

    [Fact]
    public void IsUrlValid_WhenUrlIsValidHttp_IsTrue()
    {
        var vm = CreateSut();
        vm.Url = "http://sat.local";

        vm.IsUrlValid.Should().BeTrue();
        vm.UrlValidationError.Should().BeNull();
    }

    [Fact]
    public void IsUrlValid_WhenUrlIsValidHttps_IsTrue()
    {
        var vm = CreateSut();
        vm.Url = "https://192.168.1.1/ui";

        vm.IsUrlValid.Should().BeTrue();
        vm.HasUrlValidationError.Should().BeFalse();
    }

    [Fact]
    public void IsUrlValid_WhenUrlIsEmpty_IsFalse()
    {
        var vm = CreateSut();
        vm.Url = string.Empty;

        vm.IsUrlValid.Should().BeFalse();
        vm.HasUrlValidationError.Should().BeTrue();
        vm.UrlValidationError.Should().Contain("empty");
    }

    [Fact]
    public void IsUrlValid_WhenUrlHasNonHttpScheme_IsFalse()
    {
        var vm = CreateSut();
        vm.Url = "ftp://sat.local";

        vm.IsUrlValid.Should().BeFalse();
        vm.UrlValidationError.Should().Contain("http");
    }

    [Fact]
    public void IsUrlValid_WhenUrlIsNotAValidUri_IsFalse()
    {
        var vm = CreateSut();
        vm.Url = "not a url";

        vm.IsUrlValid.Should().BeFalse();
        vm.HasUrlValidationError.Should().BeTrue();
    }

    [Fact]
    public void RetryCommand_WhenUrlIsValid_FiresRetryRequestedWithTrimmedUrl()
    {
        var vm = CreateSut();
        vm.Url = "  https://sat.local/dashboard  ";
        string? receivedUrl = null;
        vm.RetryRequested += url => receivedUrl = url;

        vm.RetryCommand.Execute(null);

        receivedUrl.Should().Be("https://sat.local/dashboard");
    }

    [Fact]
    public void RetryCommand_WhenUrlIsInvalid_CannotExecute()
    {
        var vm = CreateSut();
        vm.Url = string.Empty;

        vm.RetryCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void RetryCommand_WhenUrlIsValid_CanExecute()
    {
        var vm = CreateSut("https://sat.local");

        vm.RetryCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void CancelCommand_FiresCancelledEvent()
    {
        var vm = CreateSut();
        bool fired = false;
        vm.Cancelled += () => fired = true;

        vm.CancelCommand.Execute(null);

        fired.Should().BeTrue();
    }

    [Fact]
    public void ChangingUrl_RaisesPropertyChangedForValidationProperties()
    {
        var vm = CreateSut();
        var changedProps = new List<string?>();
        vm.PropertyChanged += (_, e) => changedProps.Add(e.PropertyName);

        vm.Url = "bad url";

        changedProps.Should().Contain(nameof(vm.UrlValidationError));
        changedProps.Should().Contain(nameof(vm.HasUrlValidationError));
        changedProps.Should().Contain(nameof(vm.IsUrlValid));
    }

    [Fact]
    public void RetryCommand_WhenUrlChangesFromInvalidToValid_BecomesExecutable()
    {
        var vm = CreateSut();
        vm.Url = string.Empty; // invalid

        vm.RetryCommand.CanExecute(null).Should().BeFalse();

        vm.Url = "https://sat.local"; // valid

        vm.RetryCommand.CanExecute(null).Should().BeTrue();
    }
}
