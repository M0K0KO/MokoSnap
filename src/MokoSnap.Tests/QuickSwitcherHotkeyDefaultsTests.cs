using MokoSnap.Core.Hotkeys;
using MokoSnap.Core.Models;

namespace MokoSnap.Tests;

public class QuickSwitcherHotkeyDefaultsTests
{
    [Fact]
    public void DefaultQuickSwitcherHotkeyIsCtrlAltM()
    {
        HotkeyGesture hotkey = QuickSwitcherHotkeyDefaults.CreateDefault();

        Assert.Equal("Ctrl+Alt+M", HotkeyGestureFormatter.Format(hotkey));
    }

    [Fact]
    public void MissingQuickSwitcherHotkeyResolvesToCtrlAltM()
    {
        HotkeyGesture hotkey = QuickSwitcherHotkeyDefaults.Resolve(null);

        Assert.Equal("Ctrl+Alt+M", HotkeyGestureFormatter.Format(hotkey));
    }

    [Fact]
    public void EmptyQuickSwitcherHotkeyResolvesToCtrlAltM()
    {
        HotkeyGesture hotkey = QuickSwitcherHotkeyDefaults.Resolve(new HotkeyGesture());

        Assert.Equal("Ctrl+Alt+M", HotkeyGestureFormatter.Format(hotkey));
    }

    [Fact]
    public void OldDefaultQuickSwitcherHotkeyMigratesToCtrlAltM()
    {
        HotkeyGesture hotkey = QuickSwitcherHotkeyDefaults.Resolve(
            HotkeyGestureFormatter.Parse("Ctrl+Alt+Space"));

        Assert.Equal("Ctrl+Alt+M", HotkeyGestureFormatter.Format(hotkey));
    }

    [Fact]
    public void CustomQuickSwitcherHotkeyIsPreserved()
    {
        HotkeyGesture hotkey = QuickSwitcherHotkeyDefaults.Resolve(
            HotkeyGestureFormatter.Parse("Ctrl+Shift+K"));

        Assert.Equal("Ctrl+Shift+K", HotkeyGestureFormatter.Format(hotkey));
    }
}
