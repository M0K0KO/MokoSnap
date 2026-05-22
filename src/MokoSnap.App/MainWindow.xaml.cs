using System.Windows;
using System.Windows.Controls;
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

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
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
        _hwndSource?.RemoveHook(WndProc);
        _hotkeyService?.Dispose();
        base.OnClosed(e);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.LoadCommand.Execute(null);
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        handled = _hotkeyService?.ProcessWindowMessage(msg, wParam) == true;
        return IntPtr.Zero;
    }

    private void OnHotkeyTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        Key key = e.Key == Key.System ? e.SystemKey : e.Key;
        ModifierKeys modifiers = Keyboard.Modifiers;
        if ((key == Key.Back || key == Key.Delete) && modifiers == ModifierKeys.None)
        {
            textBox.Text = string.Empty;
            textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
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
        textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
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
            Key.Left => "Left",
            Key.Up => "Up",
            Key.Right => "Right",
            Key.Down => "Down",
            _ => key.ToString()
        };
    }
}
