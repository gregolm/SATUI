namespace SATUI.Services;

public interface IThemeService
{
    bool IsDarkMode { get; }
    event EventHandler? ThemeChanged;
}
