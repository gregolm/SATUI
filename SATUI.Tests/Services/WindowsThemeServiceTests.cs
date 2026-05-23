using SATUI.Services;

namespace SATUI.Tests.Services;

public class WindowsThemeServiceTests
{
    [Fact]
    public void IsDarkMode_WhenReaderReturnsTrue_ReturnsTrue()
    {
        var sut = new WindowsThemeService(() => true);

        sut.IsDarkMode.ShouldBeTrue();
    }

    [Fact]
    public void IsDarkMode_WhenReaderReturnsFalse_ReturnsFalse()
    {
        var sut = new WindowsThemeService(() => false);

        sut.IsDarkMode.ShouldBeFalse();
    }

    [Fact]
    public void IsDarkMode_InvokesReaderEachCall_AllowingDynamicChanges()
    {
        bool isDark = true;
        var sut = new WindowsThemeService(() => isDark);

        sut.IsDarkMode.ShouldBeTrue();

        isDark = false;
        sut.IsDarkMode.ShouldBeFalse();
    }

    [Fact]
    public void SimulateThemeChange_NotifiesSubscribers()
    {
        var sut = new WindowsThemeService(() => true);
        bool fired = false;
        sut.ThemeChanged += (_, _) => fired = true;

        sut.SimulateThemeChange();

        fired.ShouldBeTrue();
    }

    [Fact]
    public void SimulateThemeChange_PassesServiceAsSender()
    {
        var sut = new WindowsThemeService(() => true);
        object? receivedSender = null;
        sut.ThemeChanged += (s, _) => receivedSender = s;

        sut.SimulateThemeChange();

        receivedSender.ShouldBeSameAs(sut);
    }

    [Fact]
    public void SimulateThemeChange_WithNoSubscribers_DoesNotThrow()
    {
        var sut = new WindowsThemeService(() => true);

        Should.NotThrow(() => sut.SimulateThemeChange());
    }
}
