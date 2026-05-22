using MokoSnap.Core.Capture;
using MokoSnap.Core.Models;

namespace MokoSnap.Tests;

public class CapturedWindowAppTests
{
    [Fact]
    public void ToApplicationTargetCreatesApplicationTarget()
    {
        CapturedWindowApp app = new()
        {
            WindowTitle = "Project",
            ProcessName = "Code",
            ExecutablePath = @"C:\Tools\Code.exe",
            ProcessId = 123,
            WindowHandle = 456,
            WindowPlacement = new WindowPlacementSnapshot
            {
                Enabled = true,
                ShowState = WindowPlacementShowState.Normal,
                Left = 100,
                Top = 200,
                Width = 900,
                Height = 700,
                MonitorDeviceName = @"\\.\DISPLAY1",
                WasProbablySnapped = false
            }
        };

        TargetConfig target = app.ToApplicationTarget();

        Assert.Equal(TargetType.Application, target.Type);
        Assert.Equal("Code - Project", target.DisplayName);
        Assert.Equal(@"C:\Tools\Code.exe", target.ExecutablePath);
        Assert.Equal(string.Empty, target.Arguments);
        Assert.Equal(string.Empty, target.WorkingDirectory);
        Assert.Equal(0, target.LaunchDelayMs);
        Assert.False(target.RunAsAdmin);
        Assert.NotNull(target.WindowPlacement);
        Assert.True(target.WindowPlacement.Enabled);
        Assert.Equal(WindowPlacementShowState.Normal, target.WindowPlacement.ShowState);
        Assert.Equal(100, target.WindowPlacement.Left);
        Assert.Equal(200, target.WindowPlacement.Top);
        Assert.Equal(900, target.WindowPlacement.Width);
        Assert.Equal(700, target.WindowPlacement.Height);
        Assert.Equal(@"\\.\DISPLAY1", target.WindowPlacement.MonitorDeviceName);
    }

    [Fact]
    public void IsAvailableRequiresExecutablePath()
    {
        CapturedWindowApp app = new();

        Assert.False(app.IsAvailable);

        app.ExecutablePath = @"C:\Tools\App.exe";

        Assert.True(app.IsAvailable);
    }
}
