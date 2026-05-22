using MokoSnap.Core.Models;

namespace MokoSnap.Tests;

public class WindowPlacementClampTests
{
    [Fact]
    public void ClampToVisibleWorkAreaMovesOffScreenRectToPrimaryMonitor()
    {
        WindowPlacementSnapshot snapshot = new()
        {
            Enabled = true,
            ShowState = WindowPlacementShowState.Normal,
            Left = 5000,
            Top = 3000,
            Width = 900,
            Height = 700,
            MonitorDeviceName = @"\\.\MISSING"
        };

        WindowPlacementSnapshot clamped = WindowPlacementClamp.ClampToVisibleWorkArea(
            snapshot,
            [
                new WindowPlacementWorkArea
                {
                    DeviceName = @"\\.\DISPLAY1",
                    IsPrimary = true,
                    Left = 0,
                    Top = 0,
                    Width = 1920,
                    Height = 1040
                }
            ]);

        Assert.Equal(1020, clamped.Left);
        Assert.Equal(340, clamped.Top);
        Assert.Equal(900, clamped.Width);
        Assert.Equal(700, clamped.Height);
        Assert.Equal(@"\\.\DISPLAY1", clamped.MonitorDeviceName);
    }

    [Fact]
    public void ClampToVisibleWorkAreaShrinksOversizedRect()
    {
        WindowPlacementSnapshot snapshot = new()
        {
            Left = -100,
            Top = -100,
            Width = 3000,
            Height = 2000
        };

        WindowPlacementSnapshot clamped = WindowPlacementClamp.ClampToVisibleWorkArea(
            snapshot,
            [
                new WindowPlacementWorkArea
                {
                    DeviceName = @"\\.\DISPLAY1",
                    IsPrimary = true,
                    Left = 10,
                    Top = 20,
                    Width = 1000,
                    Height = 800
                }
            ]);

        Assert.Equal(10, clamped.Left);
        Assert.Equal(20, clamped.Top);
        Assert.Equal(1000, clamped.Width);
        Assert.Equal(800, clamped.Height);
    }
}
