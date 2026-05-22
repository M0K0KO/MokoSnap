using System.Runtime.InteropServices;

namespace MokoSnap.Platform.Windows.Closing;

public sealed class WindowsGracefulWindowCloseBackend : IGracefulWindowCloseBackend
{
    private const int WmClose = 0x0010;

    public bool RequestClose(long windowHandle)
    {
        return PostMessage(new IntPtr(windowHandle), WmClose, IntPtr.Zero, IntPtr.Zero);
    }

    public bool IsWindowOpen(long windowHandle)
    {
        return IsWindow(new IntPtr(windowHandle));
    }

    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        return Task.Delay(delay, cancellationToken);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostMessage(IntPtr windowHandle, int message, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr windowHandle);
}
