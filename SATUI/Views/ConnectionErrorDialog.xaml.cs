using System.Windows;
using SATUI.ViewModels;

namespace SATUI.Views;

public partial class ConnectionErrorDialog : Window
{
    public ConnectionErrorDialog(ConnectionErrorViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
