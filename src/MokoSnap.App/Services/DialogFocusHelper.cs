using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using MokoSnap.Platform.Windows.Activation;

namespace MokoSnap.App.Services;

public static class DialogFocusHelper
{
    public static void ActivateAndFocus(Window window, FrameworkElement focusTarget)
    {
        ApplyActivation(window, focusTarget);

        window.Dispatcher.BeginInvoke(
            new Action(() => ApplyActivation(window, focusTarget)),
            DispatcherPriority.ApplicationIdle);
    }

    private static void ApplyActivation(Window window, FrameworkElement focusTarget)
    {
        if (!window.Topmost)
        {
            window.Topmost = true;
            window.Topmost = false;
        }

        IntPtr handle = new WindowInteropHelper(window).Handle;
        WindowsForegroundWindowActivator.Activate(handle);
        window.Activate();
        window.Focus();
        focusTarget.Focus();
        Keyboard.Focus(focusTarget);
    }
}
