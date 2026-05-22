using System.Runtime.InteropServices;

namespace MokoSnap.Platform.Windows.Activation;

public static class WindowsForegroundWindowActivator
{
    public static void Activate(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return;
        }

        BringWindowToTop(windowHandle);
        SetForegroundWindow(windowHandle);
    }

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr windowHandle);

    [DllImport("user32.dll")]
    private static extern bool BringWindowToTop(IntPtr windowHandle);
}
