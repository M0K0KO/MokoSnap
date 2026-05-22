using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using MokoSnap.Core.Models;

namespace MokoSnap.Platform.Windows.Placement;

public sealed class WindowsWindowPlacementService : IWindowsWindowPlacementService
{
    private const int DwmwaCloaked = 14;
    private const int DwmwaExtendedFrameBounds = 9;
    private const int GwOwner = 4;
    private const int MonitorDefaultToNearest = 2;
    private const int SwShowMinimized = 2;
    private const int SwShowMaximized = 3;
    private const int SwShowNormal = 1;
    private const uint SwpNoZOrder = 0x0004;
    private const uint SwpNoActivate = 0x0010;
    private static readonly TimeSpan RestoreTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    public WindowPlacementSnapshot? CapturePlacement(IntPtr windowHandle)
    {
        WindowPlacement placement = WindowPlacement.Create();
        if (!GetWindowPlacement(windowHandle, ref placement))
        {
            return null;
        }

        if (!TryGetWindowBounds(windowHandle, out NativeRect bounds))
        {
            return null;
        }

        WindowPlacementWorkArea? workArea = TryGetWorkArea(windowHandle);
        WindowPlacementShowState showState = ToShowState(placement.ShowCommand);

        return new WindowPlacementSnapshot
        {
            Enabled = true,
            ShowState = showState,
            Left = bounds.Left,
            Top = bounds.Top,
            Width = Math.Max(1, bounds.Right - bounds.Left),
            Height = Math.Max(1, bounds.Bottom - bounds.Top),
            MonitorDeviceName = workArea?.DeviceName ?? string.Empty,
            WasProbablySnapped = showState == WindowPlacementShowState.Normal &&
                workArea is not null &&
                IsProbablySnapped(bounds, workArea)
        };
    }

    public async Task<WindowPlacementRestoreResult> RestorePlacementAsync(
        TargetConfig target,
        int? launchedProcessId,
        CancellationToken cancellationToken = default)
    {
        if (target.WindowPlacement is null || !target.WindowPlacement.Enabled)
        {
            return WindowPlacementRestoreResult.Successful();
        }

        IntPtr windowHandle = await FindMatchingWindowAsync(target, launchedProcessId, cancellationToken);
        if (windowHandle == IntPtr.Zero)
        {
            return WindowPlacementRestoreResult.Failed("window placement restore skipped because no matching window was found");
        }

        IReadOnlyList<WindowPlacementWorkArea> workAreas = GetWorkAreas();
        WindowPlacementSnapshot clampedPlacement = WindowPlacementClamp.ClampToVisibleWorkArea(
            target.WindowPlacement,
            workAreas);

        if (!ApplyPlacement(windowHandle, clampedPlacement))
        {
            return WindowPlacementRestoreResult.Failed("window placement restore failed");
        }

        return WindowPlacementRestoreResult.Successful();
    }

    private static async Task<IntPtr> FindMatchingWindowAsync(
        TargetConfig target,
        int? launchedProcessId,
        CancellationToken cancellationToken)
    {
        DateTimeOffset deadline = DateTimeOffset.UtcNow + RestoreTimeout;
        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IntPtr matchedWindow = FindMatchingWindow(target, launchedProcessId);
            if (matchedWindow != IntPtr.Zero)
            {
                return matchedWindow;
            }

            await Task.Delay(PollInterval, cancellationToken);
        }

        return IntPtr.Zero;
    }

    private static IntPtr FindMatchingWindow(TargetConfig target, int? launchedProcessId)
    {
        IntPtr matchedWindow = IntPtr.Zero;
        string targetExecutablePath = target.ExecutablePath;
        string targetProcessName = Path.GetFileNameWithoutExtension(target.ExecutablePath);

        EnumWindows((windowHandle, _) =>
        {
            if (!IsCandidateWindow(windowHandle))
            {
                return true;
            }

            _ = GetWindowThreadProcessId(windowHandle, out int processId);
            if (launchedProcessId.HasValue && processId == launchedProcessId.Value)
            {
                matchedWindow = windowHandle;
                return false;
            }

            if (ProcessMatches(processId, targetExecutablePath, targetProcessName))
            {
                matchedWindow = windowHandle;
                return false;
            }

            return true;
        }, IntPtr.Zero);

        return matchedWindow;
    }

    private static bool IsCandidateWindow(IntPtr windowHandle)
    {
        return IsWindowVisible(windowHandle) &&
            !IsCloaked(windowHandle) &&
            GetWindow(windowHandle, GwOwner) == IntPtr.Zero &&
            GetWindowTextLength(windowHandle) > 0;
    }

    private static bool ProcessMatches(int processId, string executablePath, string processName)
    {
        try
        {
            using Process process = Process.GetProcessById(processId);
            if (!string.IsNullOrWhiteSpace(executablePath) &&
                process.MainModule?.FileName.Equals(executablePath, StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(processName) &&
                process.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static bool ApplyPlacement(IntPtr windowHandle, WindowPlacementSnapshot placement)
    {
        WindowPlacement nativePlacement = WindowPlacement.Create();
        nativePlacement.ShowCommand = ToShowCommand(placement.ShowState);
        nativePlacement.NormalPosition = new NativeRect
        {
            Left = placement.Left,
            Top = placement.Top,
            Right = placement.Left + placement.Width,
            Bottom = placement.Top + placement.Height
        };

        bool placementSet = SetWindowPlacement(windowHandle, ref nativePlacement);
        if (placement.ShowState == WindowPlacementShowState.Normal)
        {
            return placementSet &&
                SetWindowPos(
                    windowHandle,
                    IntPtr.Zero,
                    placement.Left,
                    placement.Top,
                    placement.Width,
                    placement.Height,
                    SwpNoZOrder | SwpNoActivate);
        }

        return placementSet;
    }

    private static bool TryGetWindowBounds(IntPtr windowHandle, out NativeRect bounds)
    {
        if (DwmGetWindowAttribute(
                windowHandle,
                DwmwaExtendedFrameBounds,
                out bounds,
                Marshal.SizeOf<NativeRect>()) == 0)
        {
            return true;
        }

        return GetWindowRect(windowHandle, out bounds);
    }

    private static WindowPlacementWorkArea? TryGetWorkArea(IntPtr windowHandle)
    {
        IntPtr monitorHandle = MonitorFromWindow(windowHandle, MonitorDefaultToNearest);
        return monitorHandle == IntPtr.Zero ? null : TryGetMonitorInfo(monitorHandle);
    }

    private static IReadOnlyList<WindowPlacementWorkArea> GetWorkAreas()
    {
        List<WindowPlacementWorkArea> workAreas = [];
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (monitorHandle, _, _, _) =>
        {
            WindowPlacementWorkArea? workArea = TryGetMonitorInfo(monitorHandle);
            if (workArea is not null)
            {
                workAreas.Add(workArea);
            }

            return true;
        }, IntPtr.Zero);

        return workAreas;
    }

    private static WindowPlacementWorkArea? TryGetMonitorInfo(IntPtr monitorHandle)
    {
        MonitorInfoEx info = MonitorInfoEx.Create();
        if (!GetMonitorInfo(monitorHandle, ref info))
        {
            return null;
        }

        return new WindowPlacementWorkArea
        {
            DeviceName = info.DeviceName,
            IsPrimary = (info.Flags & 1) == 1,
            Left = info.WorkArea.Left,
            Top = info.WorkArea.Top,
            Width = Math.Max(1, info.WorkArea.Right - info.WorkArea.Left),
            Height = Math.Max(1, info.WorkArea.Bottom - info.WorkArea.Top)
        };
    }

    private static bool IsProbablySnapped(NativeRect bounds, WindowPlacementWorkArea workArea)
    {
        bool touchesHorizontalEdge = bounds.Left == workArea.Left || bounds.Right == workArea.Right;
        bool touchesVerticalEdge = bounds.Top == workArea.Top || bounds.Bottom == workArea.Bottom;
        bool notFullWorkArea = bounds.Left != workArea.Left ||
            bounds.Top != workArea.Top ||
            bounds.Right != workArea.Right ||
            bounds.Bottom != workArea.Bottom;

        return notFullWorkArea && touchesHorizontalEdge && touchesVerticalEdge;
    }

    private static WindowPlacementShowState ToShowState(int showCommand)
    {
        return showCommand switch
        {
            SwShowMaximized => WindowPlacementShowState.Maximized,
            SwShowMinimized => WindowPlacementShowState.Minimized,
            _ => WindowPlacementShowState.Normal
        };
    }

    private static int ToShowCommand(WindowPlacementShowState showState)
    {
        return showState switch
        {
            WindowPlacementShowState.Maximized => SwShowMaximized,
            WindowPlacementShowState.Minimized => SwShowMinimized,
            _ => SwShowNormal
        };
    }

    private static bool IsCloaked(IntPtr windowHandle)
    {
        int result = DwmGetWindowAttribute(windowHandle, DwmwaCloaked, out int cloaked, Marshal.SizeOf<int>());
        return result == 0 && cloaked != 0;
    }

    private delegate bool EnumWindowsProc(IntPtr windowHandle, IntPtr parameter);

    private delegate bool MonitorEnumProc(
        IntPtr monitorHandle,
        IntPtr deviceContext,
        IntPtr monitorRect,
        IntPtr parameter);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr parameter);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr windowHandle);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr windowHandle);

    [DllImport("user32.dll")]
    private static extern int GetWindowThreadProcessId(IntPtr windowHandle, out int processId);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindow(IntPtr windowHandle, int command);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr windowHandle, out NativeRect bounds);

    [DllImport("user32.dll")]
    private static extern bool GetWindowPlacement(IntPtr windowHandle, ref WindowPlacement placement);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPlacement(IntPtr windowHandle, ref WindowPlacement placement);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr windowHandle,
        IntPtr insertAfter,
        int x,
        int y,
        int width,
        int height,
        uint flags);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr windowHandle, int flags);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(
        IntPtr deviceContext,
        IntPtr clippingRect,
        MonitorEnumProc callback,
        IntPtr data);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetMonitorInfo(IntPtr monitorHandle, ref MonitorInfoEx monitorInfo);

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(
        IntPtr windowHandle,
        int attribute,
        out int value,
        int size);

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(
        IntPtr windowHandle,
        int attribute,
        out NativeRect value,
        int size);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowPlacement
    {
        public int Length;
        public int Flags;
        public int ShowCommand;
        public NativePoint MinPosition;
        public NativePoint MaxPosition;
        public NativeRect NormalPosition;

        public static WindowPlacement Create()
        {
            return new WindowPlacement { Length = Marshal.SizeOf<WindowPlacement>() };
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct MonitorInfoEx
    {
        private const int DeviceNameLength = 32;

        public int Size;
        public NativeRect MonitorArea;
        public NativeRect WorkArea;
        public int Flags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = DeviceNameLength)]
        public string DeviceName;

        public static MonitorInfoEx Create()
        {
            return new MonitorInfoEx
            {
                Size = Marshal.SizeOf<MonitorInfoEx>(),
                DeviceName = string.Empty
            };
        }
    }
}
