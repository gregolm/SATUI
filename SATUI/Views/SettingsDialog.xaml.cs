using System.ComponentModel;
using System.Windows;
using SATUI.ViewModels;

namespace SATUI.Views;

public partial class SettingsDialog : Window
{
    private readonly SettingsViewModel _viewModel;
    private bool _okToClose;

    public SettingsDialog(SettingsViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();

        // Allow close when any command completes successfully
        viewModel.SettingsSaved += _ => _okToClose = true;
        viewModel.Cancelled += () => _okToClose = true;
        viewModel.ExitRequested += () => _okToClose = true;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Block the window chrome X-button when cancel is not permitted
        if (!_viewModel.AllowCancel && !_okToClose)
            e.Cancel = true;

        base.OnClosing(e);
    }
}

