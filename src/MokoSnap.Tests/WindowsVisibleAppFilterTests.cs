using MokoSnap.Core.Capture;
using MokoSnap.Core.Models;
using MokoSnap.Platform.Windows.Capture;

namespace MokoSnap.Tests;

public class WindowsVisibleAppFilterTests
{
    [Fact]
    public void ShouldIncludeRejectsEmptyTitles()
    {
        WindowsVisibleAppFilter filter = new();

        bool include = filter.ShouldInclude(new WindowsCapturedWindowInfo
        {
            WindowTitle = "",
            ProcessName = "notepad",
            ExecutablePath = @"C:\Windows\notepad.exe"
        }, new VisibleAppCaptureOptions());

        Assert.False(include);
    }

    [Fact]
    public void ShouldIncludeRejectsMokoSnap()
    {
        WindowsVisibleAppFilter filter = new();

        bool include = filter.ShouldInclude(new WindowsCapturedWindowInfo
        {
            WindowTitle = "MokoSnap",
            ProcessName = "MokoSnap.App",
            ExecutablePath = @"C:\MokoSnap\MokoSnap.App.exe"
        }, new VisibleAppCaptureOptions());

        Assert.False(include);
    }

    [Fact]
    public void ShouldIncludeRejectsSystemUiWindows()
    {
        WindowsVisibleAppFilter filter = new();

        bool include = filter.ShouldInclude(new WindowsCapturedWindowInfo
        {
            WindowTitle = "Taskbar",
            ProcessName = "explorer",
            ClassName = "Shell_TrayWnd"
        }, new VisibleAppCaptureOptions { IncludeExplorer = true });

        Assert.False(include);
    }

    [Fact]
    public void ShouldIncludeRejectsExplorerByDefault()
    {
        WindowsVisibleAppFilter filter = new();

        bool include = filter.ShouldInclude(new WindowsCapturedWindowInfo
        {
            WindowTitle = "Downloads",
            ProcessName = "explorer",
            ClassName = "CabinetWClass"
        }, new VisibleAppCaptureOptions());

        Assert.False(include);
    }

    [Fact]
    public void ShouldIncludeAllowsExplorerWhenRequested()
    {
        WindowsVisibleAppFilter filter = new();

        bool include = filter.ShouldInclude(new WindowsCapturedWindowInfo
        {
            WindowTitle = "Downloads",
            ProcessName = "explorer",
            ClassName = "CabinetWClass"
        }, new VisibleAppCaptureOptions { IncludeExplorer = true });

        Assert.True(include);
    }

    [Fact]
    public void ToCapturedAppMarksExplorer()
    {
        WindowsVisibleAppFilter filter = new();

        CapturedWindowApp app = filter.ToCapturedApp(new WindowsCapturedWindowInfo
        {
            WindowTitle = "Downloads",
            ProcessName = "explorer",
            ExecutablePath = @"C:\Windows\explorer.exe",
            ProcessId = 100,
            WindowHandle = 200,
            WindowPlacement = new WindowPlacementSnapshot
            {
                Enabled = true,
                ShowState = WindowPlacementShowState.Normal,
                Left = 1,
                Top = 2,
                Width = 300,
                Height = 400,
                MonitorDeviceName = @"\\.\DISPLAY1"
            }
        });

        Assert.True(app.IsExplorer);
        Assert.Equal("Downloads", app.WindowTitle);
        Assert.Equal("explorer", app.ProcessName);
        Assert.Equal(@"C:\Windows\explorer.exe", app.ExecutablePath);
        Assert.NotNull(app.WindowPlacement);
        Assert.Equal(1, app.WindowPlacement.Left);
        Assert.Equal(2, app.WindowPlacement.Top);
        Assert.Equal(300, app.WindowPlacement.Width);
        Assert.Equal(400, app.WindowPlacement.Height);
    }
}
