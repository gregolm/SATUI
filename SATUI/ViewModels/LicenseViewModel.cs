using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SATUI.ViewModels;

public partial class LicenseViewModel : ObservableObject
{
    public string LicenseText { get; } = AppLicense.Text;

    public event Action? LicenseAccepted;
    public event Action? LicenseDeclined;

    [RelayCommand]
    public void Accept()
    {
        LicenseAccepted?.Invoke();
    }

    [RelayCommand]
    public void Decline()
    {
        LicenseDeclined?.Invoke();
    }
}
