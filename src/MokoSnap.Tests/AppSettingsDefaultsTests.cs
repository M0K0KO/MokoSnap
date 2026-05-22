using MokoSnap.Core.Hotkeys;
using MokoSnap.Core.Models;

namespace MokoSnap.Tests;

public class AppSettingsDefaultsTests
{
    [Fact]
    public void DefaultSettingsUseCtrlAltMQuickSwitcherHotkey()
    {
        AppSettings settings = AppSettings.CreateDefault();

        Assert.Equal("Ctrl+Alt+M", HotkeyGestureFormatter.Format(settings.QuickSwitcherHotkey));
    }

    [Fact]
    public void DefaultSettingsMinimizeToTray()
    {
        AppSettings settings = AppSettings.CreateDefault();

        Assert.True(settings.MinimizeToTray);
        Assert.False(settings.StartMinimizedToTray);
        Assert.False(settings.LaunchOnStartup);
    }
}
