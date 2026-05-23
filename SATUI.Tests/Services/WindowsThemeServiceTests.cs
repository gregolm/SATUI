using FluentAssertions;
using SATUI.Services;

namespace SATUI.Tests.Services;

public class WindowsThemeServiceTests
{
    [Fact]
    public void IsDarkMode_WhenReaderReturnsTrue_ReturnsTrue()
    {
        var sut = new WindowsThemeService(() => true);

        sut.IsDarkMode.Should().BeTrue();
    }

    [Fact]
    public void IsDarkMode_WhenReaderReturnsFalse_ReturnsFalse()
    {
        var sut = new WindowsThemeService(() => false);

        sut.IsDarkMode.Should().BeFalse();
    }

    [Fact]
    public void IsDarkMode_InvokesReaderEachCall_AllowingDynamicChanges()
    {
        bool isDark = true;
        var sut = new WindowsThemeService(() => isDark);

        sut.IsDarkMode.Should().BeTrue();

        isDark = false;
        sut.IsDarkMode.Should().BeFalse();
    }

    [Fact]
    public void SimulateThemeChange_NotifiesSubscribers()
    {
        var sut = new WindowsThemeService(() => true);
        bool fired = false;
        sut.ThemeChanged += (_, _) => fired = true;

        sut.SimulateThemeChange();

        fired.Should().BeTrue();
    }

    [Fact]
    public void SimulateThemeChange_PassesServiceAsSender()
    {
        var sut = new WindowsThemeService(() => true);
        object? receivedSender = null;
        sut.ThemeChanged += (s, _) => receivedSender = s;

        sut.SimulateThemeChange();

        receivedSender.Should().BeSameAs(sut);
    }

    [Fact]
    public void SimulateThemeChange_WithNoSubscribers_DoesNotThrow()
    {
        var sut = new WindowsThemeService(() => true);

        var act = () => sut.SimulateThemeChange();

        act.Should().NotThrow();
    }
}
