using System.Windows;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;
using SATUI.Models;
using SATUI.Services;
using SATUI.ViewModels;
using SATUI.Views;

namespace SATUI;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly ISettingsService _settingsService;
    private bool _isFullScreen;
    private WindowState _savedWindowState;
    private WindowStyle _savedWindowStyle;
    private ResizeMode _savedResizeMode;

    public MainWindow(MainViewModel viewModel, ISettingsService settingsService)
    {
        _viewModel = viewModel;
        _settingsService = settingsService;
        DataContext = viewModel;

        InitializeComponent();

        viewModel.NavigationRequested += OnNavigationRequested;
        viewModel.OpenSettingsRequested += allowCancel => OpenSettings(allowCancel);
        viewModel.ConnectionErrorRequested += OpenConnectionErrorDialog;
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        await WebView.EnsureCoreWebView2Async();
        WebView.NavigationCompleted += OnWebViewNavigationCompleted;
        WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        WebView.CoreWebView2.Settings.AreDevToolsEnabled = false;

        await _viewModel.InitializeAsync();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.F11)
        {
            ToggleFullScreen();
            e.Handled = true;
        }
        base.OnPreviewKeyDown(e);
    }

    private void OnNavigationRequested(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
            WebView.Source = new Uri(url);
    }

    private void OnWebViewNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (!e.IsSuccess)
            _viewModel.OnNavigationFailed(WebView.Source?.ToString() ?? string.Empty,
                e.WebErrorStatus.ToString());
    }

    private void OpenConnectionErrorDialog(string attemptedUrl)
    {
        var errorVm = new ConnectionErrorViewModel(attemptedUrl);
        var dialog = new ConnectionErrorDialog(errorVm);
        dialog.Owner = this;

        void OnRetry(string newUrl)
        {
            errorVm.RetryRequested -= OnRetry;
            errorVm.Cancelled -= OnCancelled;
            dialog.Close();

            _viewModel.ApplySettings(new AppSettings { Url = newUrl });
            _ = _viewModel.NavigateAsync();
        }

        void OnCancelled()
        {
            errorVm.RetryRequested -= OnRetry;
            errorVm.Cancelled -= OnCancelled;
            dialog.Close();
        }

        errorVm.RetryRequested += OnRetry;
        errorVm.Cancelled += OnCancelled;
        dialog.ShowDialog();
    }

    private void OpenSettings(bool allowCancel = true)
    {
        var settingsVm = new SettingsViewModel(_settingsService, allowCancel);
        var dialog = new SettingsDialog(settingsVm);
        dialog.Owner = this;

        void OnSaved(AppSettings settings)
        {
            settingsVm.SettingsSaved -= OnSaved;
            settingsVm.Cancelled -= OnCancelled;
            settingsVm.ExitRequested -= OnExitRequested;
            _viewModel.ApplySettings(settings);
            dialog.Close();
            _ = _viewModel.NavigateAsync();
        }

        void OnCancelled()
        {
            settingsVm.SettingsSaved -= OnSaved;
            settingsVm.Cancelled -= OnCancelled;
            settingsVm.ExitRequested -= OnExitRequested;
            dialog.Close();
        }

        void OnExitRequested()
        {
            settingsVm.SettingsSaved -= OnSaved;
            settingsVm.Cancelled -= OnCancelled;
            settingsVm.ExitRequested -= OnExitRequested;
            dialog.Close();
            Application.Current.Shutdown();
        }

        settingsVm.SettingsSaved += OnSaved;
        settingsVm.Cancelled += OnCancelled;
        settingsVm.ExitRequested += OnExitRequested;
        dialog.ShowDialog();
    }

    private void ToggleFullScreen()
    {
        if (!_isFullScreen)
        {
            _savedWindowState = WindowState;
            _savedWindowStyle = WindowStyle;
            _savedResizeMode = ResizeMode;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
        }
        else
        {
            WindowStyle = _savedWindowStyle;
            ResizeMode = _savedResizeMode;
            WindowState = _savedWindowState;
        }
        _isFullScreen = !_isFullScreen;
    }
}