using FluentAssertions;
using Moq;
using SATUI.Models;
using SATUI.Services;
using SATUI.ViewModels;

namespace SATUI.Tests.ViewModels;

public class MainViewModelTests
{
    private static (MainViewModel vm, Mock<ISettingsService> settingsMock, Mock<IConnectivityService> connectivityMock)
        CreateSut(string storedUrl = "https://www.example.com", bool isReachable = true)
    {
        var settingsMock = new Mock<ISettingsService>();
        settingsMock.Setup(s => s.Load()).Returns(new AppSettings { Url = storedUrl });

        var connectivityMock = new Mock<IConnectivityService>();
        connectivityMock
            .Setup(c => c.IsReachableAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(isReachable);

        var vm = new MainViewModel(settingsMock.Object, connectivityMock.Object);
        vm.CurrentUrl = storedUrl; // pre-set so tests calling NavigateAsync directly have a URL
        return (vm, settingsMock, connectivityMock);
    }

    [Fact]
    public async Task InitializeAsync_LoadsUrlFromSettings()
    {
        var (vm, _, _) = CreateSut("https://test.satui.com");

        await vm.InitializeAsync();

        vm.CurrentUrl.Should().Be("https://test.satui.com");
    }

    [Fact]
    public async Task InitializeAsync_WhenReachable_FiresNavigationRequested()
    {
        var (vm, _, _) = CreateSut(isReachable: true);
        string? navigatedTo = null;
        vm.NavigationRequested += url => navigatedTo = url;

        await vm.InitializeAsync();

        navigatedTo.Should().Be("https://www.example.com");
    }

    [Fact]
    public async Task InitializeAsync_WhenUnreachable_FiresConnectionErrorRequestedAndDoesNotNavigate()
    {
        var (vm, _, _) = CreateSut(isReachable: false);
        bool navigated = false;
        string? errorUrl = null;
        vm.NavigationRequested += _ => navigated = true;
        vm.ConnectionErrorRequested += url => errorUrl = url;

        await vm.InitializeAsync();

        navigated.Should().BeFalse();
        errorUrl.Should().Be("https://www.example.com");
    }

    [Fact]
    public async Task InitializeAsync_WhenUrlIsEmpty_FiresOpenSettingsRequestedAndSkipsConnectivity()
    {
        var (vm, _, connectivityMock) = CreateSut(storedUrl: string.Empty);
        bool settingsFired = false;
        vm.OpenSettingsRequested += () => settingsFired = true;

        await vm.InitializeAsync();

        settingsFired.Should().BeTrue();
        connectivityMock.Verify(
            c => c.IsReachableAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task NavigateAsync_WhenUrlIsEmpty_FiresOpenSettingsRequestedAndSkipsConnectivity()
    {
        var (vm, _, connectivityMock) = CreateSut();
        vm.CurrentUrl = string.Empty;
        bool settingsFired = false;
        vm.OpenSettingsRequested += () => settingsFired = true;

        await vm.NavigateAsync();

        settingsFired.Should().BeTrue();
        connectivityMock.Verify(
            c => c.IsReachableAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task NavigateAsync_WhenUnreachable_FiresConnectionErrorRequestedWithUrl()
    {
        var (vm, _, _) = CreateSut(isReachable: false);
        string? errorUrl = null;
        vm.ConnectionErrorRequested += url => errorUrl = url;

        await vm.NavigateAsync();

        errorUrl.Should().Be("https://www.example.com");
    }

    [Fact]
    public async Task NavigateAsync_WhenReachable_FiresNavigationRequestedAndNotConnectionError()
    {
        var (vm, _, _) = CreateSut(isReachable: true);
        bool connectionErrorFired = false;
        string? navigatedTo = null;
        vm.NavigationRequested += url => navigatedTo = url;
        vm.ConnectionErrorRequested += _ => connectionErrorFired = true;

        await vm.NavigateAsync();

        navigatedTo.Should().Be("https://www.example.com");
        connectionErrorFired.Should().BeFalse();
    }

    [Fact]
    public async Task NavigateAsync_SetsIsLoadingDuringCheck_ThenClearsIt()
    {
        var tcs = new TaskCompletionSource<bool>();
        var settingsMock = new Mock<ISettingsService>();
        settingsMock.Setup(s => s.Load()).Returns(new AppSettings { Url = "https://example.com" });
        var connectivityMock = new Mock<IConnectivityService>();
        connectivityMock
            .Setup(c => c.IsReachableAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        var vm = new MainViewModel(settingsMock.Object, connectivityMock.Object);
        vm.CurrentUrl = "https://example.com";

        var navigateTask = vm.NavigateAsync();
        vm.IsLoading.Should().BeTrue();

        tcs.SetResult(true);
        await navigateTask;

        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void OnNavigationFailed_FiresConnectionErrorRequestedWithUrl()
    {
        var (vm, _, _) = CreateSut();
        string? errorUrl = null;
        vm.ConnectionErrorRequested += url => errorUrl = url;

        vm.OnNavigationFailed("https://example.com", "ERR_NAME_NOT_RESOLVED");

        errorUrl.Should().Be("https://example.com");
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void ApplySettings_UpdatesCurrentUrl()
    {
        var (vm, _, _) = CreateSut();

        vm.ApplySettings(new AppSettings { Url = "https://new.url.com" });

        vm.CurrentUrl.Should().Be("https://new.url.com");
    }

    [Fact]
    public void OpenSettingsCommand_FiresOpenSettingsRequestedEvent()
    {
        var (vm, _, _) = CreateSut();
        bool fired = false;
        vm.OpenSettingsRequested += () => fired = true;

        vm.OpenSettingsCommand.Execute(null);

        fired.Should().BeTrue();
    }

    [Fact]
    public async Task NavigateAsync_WhenFirstCandidateUnreachableButSecondReachable_NavigatesToSecond()
    {
        var settingsMock = new Mock<ISettingsService>();
        settingsMock.Setup(s => s.Load()).Returns(new AppSettings { Url = "192.168.1.100" });

        var connectivityMock = new Mock<IConnectivityService>();
        connectivityMock
            .Setup(c => c.IsReachableAsync("https://192.168.1.100", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        connectivityMock
            .Setup(c => c.IsReachableAsync("http://192.168.1.100", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var vm = new MainViewModel(settingsMock.Object, connectivityMock.Object);
        vm.CurrentUrl = "192.168.1.100";

        string? navigatedTo = null;
        bool connectionErrorFired = false;
        vm.NavigationRequested += url => navigatedTo = url;
        vm.ConnectionErrorRequested += _ => connectionErrorFired = true;

        await vm.NavigateAsync();

        navigatedTo.Should().Be("http://192.168.1.100");
        connectionErrorFired.Should().BeFalse();
    }

    [Fact]
    public async Task NavigateAsync_WhenAllCandidatesUnreachable_FiresConnectionError()
    {
        var settingsMock = new Mock<ISettingsService>();
        settingsMock.Setup(s => s.Load()).Returns(new AppSettings { Url = "192.168.1.100" });

        var connectivityMock = new Mock<IConnectivityService>();
        connectivityMock
            .Setup(c => c.IsReachableAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var vm = new MainViewModel(settingsMock.Object, connectivityMock.Object);
        vm.CurrentUrl = "192.168.1.100";

        string? errorUrl = null;
        vm.ConnectionErrorRequested += url => errorUrl = url;

        await vm.NavigateAsync();

        errorUrl.Should().Be("192.168.1.100");  // raw input, not a candidate URL
    }
}

