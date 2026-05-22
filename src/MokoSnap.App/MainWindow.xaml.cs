using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Interop;
using System.Windows.Input;
using MokoSnap.App.Services;
using MokoSnap.App.ViewModels;
using MokoSnap.Core.Running;
using MokoSnap.Core.Storage;
using MokoSnap.Platform.Windows.Closing;
using MokoSnap.Platform.Windows.Capture;
using MokoSnap.Platform.Windows.Hotkeys;
using MokoSnap.Platform.Windows.Launching;

namespace MokoSnap.App;

public partial class MainWindow : Window
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
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        IntPtr windowHandle = new WindowInteropHelper(this).Handle;
        _hotkeyService = new WindowsGlobalHotkeyService(windowHandle);
        _hwndSource = HwndSource.FromHwnd(windowHandle);
        _hwndSource?.AddHook(WndProc);

        DataContext = new MainViewModel(
            MokoSnapStoragePaths.CreateAppSettingsStorage(),
            new MessageBoxConfirmationService(),
            new CapturedAppSelectionService(new WindowsVisibleAppCaptureService()),
            new PresetRunnerService(
                new WindowsTargetLauncher(),
                new WindowsVisibleWindowCloser(new CloseWindowsConfirmationService()),
                new SystemLaunchDelay()),
            _hotkeyService,
            new CommandPaletteService(this));
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
        if (!_isExplicitExit)
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

        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or
            Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin)
        {
            e.Handled = true;
            return;
        }

        List<string> parts = [];
        if (modifiers.HasFlag(ModifierKeys.Control))
        {
            parts.Add("Ctrl");
        }

        if (modifiers.HasFlag(ModifierKeys.Alt))
        {
            parts.Add("Alt");
        }

        if (modifiers.HasFlag(ModifierKeys.Shift))
        {
            parts.Add("Shift");
        }

        if (modifiers.HasFlag(ModifierKeys.Windows))
        {
            parts.Add("Win");
        }

        parts.Add(FormatRecordedKey(key));
        textBox.Text = string.Join("+", parts);
        textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty)?.UpdateSource();
        e.Handled = true;
    }

    private static string FormatRecordedKey(Key key)
    {
        return key switch
        {
            Key.Back => "Backspace",
            Key.Return => "Enter",
            Key.Escape => "Escape",
            Key.Space => "Space",
            Key.Prior => "PageUp",
            Key.Next => "PageDown",
            Key.Delete => "Delete",
            Key.Insert => "Insert",
            Key.D0 => "0",
            Key.D1 => "1",
            Key.D2 => "2",
            Key.D3 => "3",
            Key.D4 => "4",
            Key.D5 => "5",
            Key.D6 => "6",
            Key.D7 => "7",
            Key.D8 => "8",
            Key.D9 => "9",
            Key.Left => "Left",
            Key.Up => "Up",
            Key.Right => "Right",
            Key.Down => "Down",
            _ => key.ToString()
        };
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
