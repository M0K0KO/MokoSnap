using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Interop;
using System.Windows.Input;
using MokoSnap.App.Services;
using MokoSnap.App.ViewModels;
using MokoSnap.Core.ChromeCapture;
using MokoSnap.Core.Running;
using MokoSnap.Core.Storage;
using MokoSnap.Platform.Windows.Closing;
using MokoSnap.Platform.Windows.Capture;
using MokoSnap.Platform.Windows.ChromeCapture;
using MokoSnap.Platform.Windows.Hotkeys;
using MokoSnap.Platform.Windows.Launching;
using MokoSnap.Platform.Windows.Startup;
using Wpf.Ui.Controls;

namespace MokoSnap.App;

public partial class MainWindow : FluentWindow
{
    private WindowsGlobalHotkeyService? _hotkeyService;
    private HwndSource? _hwndSource;
    private TrayIconService? _trayIconService;
    private bool _isExplicitExit;

    public MainWindow()
    {
        InitializeComponent();
        StateChanged += OnStateChanged;
    }

    public async Task StartAsync(bool startMinimizedRequested)
    {
        new WindowInteropHelper(this).EnsureHandle();

        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        await viewModel.LoadAsync();
        EnsureTrayIcon(viewModel);

        if (startMinimizedRequested || viewModel.StartMinimizedToTray)
        {
            Hide();
            return;
        }

        ShowMainWindow();
        await viewModel.ShowFirstRunOnboardingIfNeededAsync(false);
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        IntPtr windowHandle = new WindowInteropHelper(this).Handle;
        _hotkeyService = new WindowsGlobalHotkeyService(windowHandle);
        _hwndSource = HwndSource.FromHwnd(windowHandle);
        _hwndSource?.AddHook(WndProc);
        ChromeNativeHostSetupService chromeNativeHostSetupService = new();
        ChromeNativeHostSetupDialogService chromeNativeHostSetupDialogService = new(
            this,
            chromeNativeHostSetupService);
        ChromeCaptureDiagnosticsService chromeCaptureDiagnosticsService = new(chromeNativeHostSetupService);

        DataContext = new MainViewModel(
            MokoSnapStoragePaths.CreateAppSettingsStorage(),
            new MessageBoxConfirmationService(),
            new CapturedAppSelectionService(new WindowsVisibleAppCaptureService()),
            new ChromeTabCaptureSelectionService(
                new ChromeTabCaptureStorage(MokoSnapStoragePaths.ChromeTabsLatestPath)),
            chromeNativeHostSetupDialogService,
            new PresetRunnerService(
                new WindowsTargetLauncher(),
                new WindowsVisibleWindowCloser(new CloseWindowsConfirmationService()),
                new SystemLaunchDelay()),
            _hotkeyService,
            new CommandPaletteService(this),
            new WindowsStartupRegistrationService(),
            new SettingsDialogService(
                this,
                chromeNativeHostSetupDialogService,
                chromeNativeHostSetupService),
            new OnboardingDialogService(this),
            chromeCaptureDiagnosticsService);
    }

    protected override void OnClosed(EventArgs e)
    {
        _trayIconService?.Dispose();
        _hwndSource?.RemoveHook(WndProc);
        _hotkeyService?.Dispose();
        base.OnClosed(e);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isExplicitExit &&
            DataContext is MainViewModel viewModel &&
            viewModel.MinimizeToTray)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        handled = _hotkeyService?.ProcessWindowMessage(msg, wParam) == true;
        return IntPtr.Zero;
    }

    private void OnHotkeyTextBoxPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (sender is not System.Windows.Controls.TextBox textBox)
        {
            return;
        }

        Key key = e.Key == Key.System ? e.SystemKey : e.Key;
        ModifierKeys modifiers = Keyboard.Modifiers;
        if ((key == Key.Back || key == Key.Delete) && modifiers == ModifierKeys.None)
        {
            textBox.Text = string.Empty;
            textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty)?.UpdateSource();
            e.Handled = true;
            return;
        }

        if (WpfHotkeyRecorder.IsModifierKey(key))
        {
            e.Handled = true;
            return;
        }

        textBox.Text = WpfHotkeyRecorder.FormatRecordedHotkey(key, modifiers);
        textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty)?.UpdateSource();
        e.Handled = true;
    }

    private void EnsureTrayIcon(MainViewModel viewModel)
    {
        if (_trayIconService is not null)
        {
            return;
        }

        _trayIconService = new TrayIconService(
            () => viewModel.Presets.ToList(),
            ShowMainWindow,
            viewModel.OpenCommandPaletteAsync,
            viewModel.OpenSettingsAsync,
            viewModel.RunPresetByIdAsync,
            ExitFromTray);
    }

    private void ShowMainWindow()
    {
        Show();
        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        DialogFocusHelper.ActivateAndFocus(this, this);
    }

    public void ActivateFromExternalLaunch()
    {
        ShowMainWindow();
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.NoteActivationRequest();
        }
    }

    private void ExitFromTray()
    {
        _isExplicitExit = true;
        _trayIconService?.Dispose();
        _trayIconService = null;
        Close();
        System.Windows.Application.Current.Shutdown();
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        if (WindowState != WindowState.Minimized ||
            DataContext is not MainViewModel viewModel ||
            !viewModel.MinimizeToTray)
        {
            return;
        }

        Hide();
    }
}
