using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SATUI.Services;

namespace SATUI.ViewModels;

/// <summary>
/// Backs the connection-error dialog shown when a navigation attempt fails.
/// Displays the URL that was tried and lets the user correct it before retrying.
/// </summary>
public partial class ConnectionErrorViewModel : ObservableObject
{
    /// <summary>The URL that was attempted — shown read-only for reference.</summary>
    public string AttemptedUrl { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RetryCommand))]
    private string _url;

    /// <summary>Raised when the user wants to retry with <see cref="Url"/>.</summary>
    public event Action<string>? RetryRequested;

    /// <summary>Raised when the user cancels without retrying.</summary>
    public event Action? Cancelled;

    public ConnectionErrorViewModel(string attemptedUrl)
    {
        AttemptedUrl = attemptedUrl;
        _url = attemptedUrl;
    }

    public string? UrlValidationError => UrlNormalizer.Validate(Url);

    public bool HasUrlValidationError => UrlValidationError is not null;

    public bool IsUrlValid => UrlValidationError is null;

    public string? UrlHint => UrlNormalizer.GetHint(Url);

    public bool HasUrlHint => UrlHint is not null;

    [RelayCommand(CanExecute = nameof(IsUrlValid))]
    public void Retry()
    {
        RetryRequested?.Invoke(Url.Trim());
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
        OnPropertyChanged(nameof(IsUrlValid));
        OnPropertyChanged(nameof(UrlHint));
        OnPropertyChanged(nameof(HasUrlHint));
    }
}
