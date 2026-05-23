using System.ComponentModel;
using System.Windows;
using SATUI.ViewModels;

namespace SATUI.Views;

public partial class LicenseDialog : Window
{
    private bool _okToClose;

    public LicenseDialog(LicenseViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();

        // Allow close only after Accept or Decline is explicitly chosen
        viewModel.LicenseAccepted += () => _okToClose = true;
        viewModel.LicenseDeclined += () => _okToClose = true;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Block the window chrome X-button — user must Accept or Decline
        if (!_okToClose)
            e.Cancel = true;

        base.OnClosing(e);
    }
}
