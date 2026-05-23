using System.Windows;
using SATUI.ViewModels;

namespace SATUI.Views;

public partial class SettingsDialog : Window
{
    public SettingsDialog(SettingsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
