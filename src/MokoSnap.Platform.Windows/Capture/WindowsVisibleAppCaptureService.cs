using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using MokoSnap.Core.Capture;
using MokoSnap.Platform.Windows.Placement;

namespace MokoSnap.Platform.Windows.Capture;

public sealed class WindowsVisibleAppCaptureService : IVisibleAppCaptureService
{
    private const int GwOwner = 4;
    private const int DwmwaCloaked = 14;

    private readonly WindowsVisibleAppFilter _filter;
    private readonly IWindowsWindowPlacementService _placementService;

    public WindowsVisibleAppCaptureService(
        WindowsVisibleAppFilter? filter = null,
        IWindowsWindowPlacementService? placementService = null)
    {
        _filter = filter ?? new WindowsVisibleAppFilter();
        _placementService = placementService ?? new WindowsWindowPlacementService();
    }

    public Task<IReadOnlyList<CapturedWindowApp>> CaptureVisibleAppsAsync(
        VisibleAppCaptureOptions options,
        CancellationToken cancellationToken = default)
    {
        List<CapturedWindowApp> apps = [];

        EnumWindows((windowHandle, _) =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            WindowsCapturedWindowInfo? window = TryCaptureWindow(windowHandle, _placementService);
            if (window is not null && _filter.ShouldInclude(window, options))
            {
                apps.Add(_filter.ToCapturedApp(window));
            }

            return true;
        }, IntPtr.Zero);

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<CapturedWindowApp>>(apps);
    }

    private static WindowsCapturedWindowInfo? TryCaptureWindow(
        IntPtr windowHandle,
        IWindowsWindowPlacementService placementService)
    {
        if (!IsWindowVisible(windowHandle) || IsCloaked(windowHandle) || GetWindow(windowHandle, GwOwner) != IntPtr.Zero)
        {
            return null;
        }

        string title = GetWindowTitle(windowHandle);
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        _ = GetWindowThreadProcessId(windowHandle, out int processId);
        string processName = string.Empty;
        string executablePath = string.Empty;

        try
        {
            using Process process = Process.GetProcessById(processId);
            processName = process.ProcessName;
            executablePath = process.MainModule?.FileName ?? string.Empty;
        }
        catch
        {
            // Some protected processes do not expose module paths. Keep the item available for preview.
        }

        return new WindowsCapturedWindowInfo
        {
            WindowTitle = title,
            ProcessName = processName,
            ExecutablePath = executablePath,
            ProcessId = processId,
            WindowHandle = windowHandle.ToInt64(),
            ClassName = GetWindowClassName(windowHandle),
            WindowPlacement = placementService.CapturePlacement(windowHandle)
        };
    }

    private static string GetWindowTitle(IntPtr windowHandle)
    {
        int length = GetWindowTextLength(windowHandle);
        if (length <= 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new(length + 1);
        _ = GetWindowText(windowHandle, builder, builder.Capacity);
        return builder.ToString();
    }

    private static string GetWindowClassName(IntPtr windowHandle)
    {
        StringBuilder builder = new(256);
        _ = GetClassName(windowHandle, builder, builder.Capacity);
        return builder.ToString();
    }

    private static bool IsCloaked(IntPtr windowHandle)
    {
        int result = DwmGetWindowAttribute(windowHandle, DwmwaCloaked, out int cloaked, Marshal.SizeOf<int>());
        return result == 0 && cloaked != 0;
    }

    private delegate bool EnumWindowsProc(IntPtr windowHandle, IntPtr parameter);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr parameter);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr windowHandle);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr windowHandle);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr windowHandle, StringBuilder text, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr windowHandle, StringBuilder className, int maxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowThreadProcessId(IntPtr windowHandle, out int processId);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindow(IntPtr windowHandle, int command);

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(
        IntPtr windowHandle,
        int attribute,
        out int value,
        int size);
}
