using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SATUI.Models;
using SATUI.Services;

namespace SATUI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private AppSettings _currentSettings;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _url = string.Empty;

    public bool AllowCancel { get; }

    public string SecondaryButtonText => AllowCancel ? "Cancel" : "Exit Application";

    // Raised when settings are saved (passes new settings to caller)
    public event Action<AppSettings>? SettingsSaved;

    // Raised when user cancels (only when AllowCancel = true)
    public event Action? Cancelled;

    // Raised when user exits (only when AllowCancel = false)
    public event Action? ExitRequested;

    public SettingsViewModel(ISettingsService settingsService, bool allowCancel = true)
    {
        _settingsService = settingsService;
        AllowCancel = allowCancel;
        _currentSettings = _settingsService.Load();
        _url = _currentSettings.Url;
    }

    public string? UrlValidationError => UrlNormalizer.Validate(Url);

    public bool HasUrlValidationError => UrlValidationError is not null;

    public bool IsValid => UrlValidationError is null;

    public string? UrlHint => UrlNormalizer.GetHint(Url);

    public bool HasUrlHint => UrlHint is not null;

    [RelayCommand(CanExecute = nameof(IsValid))]
    public void Save()
    {
        _currentSettings.Url = Url.Trim();
        _settingsService.Save(_currentSettings);
        SettingsSaved?.Invoke(_currentSettings);
    }

    [RelayCommand]
    public void Cancel()
    {
        if (AllowCancel)
            Cancelled?.Invoke();
        else
            ExitRequested?.Invoke();
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

