using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SATUI.Models;
using SATUI.Services;

namespace SATUI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _url = string.Empty;

    // Raised when settings are saved (passes new settings to caller)
    public event Action<AppSettings>? SettingsSaved;

    // Raised when user cancels
    public event Action? Cancelled;

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        var settings = _settingsService.Load();
        _url = settings.Url;
    }

    public string? UrlValidationError => UrlNormalizer.Validate(Url);

    public bool HasUrlValidationError => UrlValidationError is not null;

    public bool IsValid => UrlValidationError is null;

    public string? UrlHint => UrlNormalizer.GetHint(Url);

    public bool HasUrlHint => UrlHint is not null;

    [RelayCommand(CanExecute = nameof(IsValid))]
    public void Save()
    {
        var settings = new AppSettings { Url = Url.Trim() };
        _settingsService.Save(settings);
        SettingsSaved?.Invoke(settings);
    }

    [RelayCommand]
    public void Cancel()
    {
        Cancelled?.Invoke();
    }

    partial void OnUrlChanged(string value)
    {
        OnPropertyChanged(nameof(UrlValidationError));
        OnPropertyChanged(nameof(HasUrlValidationError));
        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(UrlHint));
        OnPropertyChanged(nameof(HasUrlHint));
    }
}
