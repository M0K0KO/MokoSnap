using MokoSnap.Core.Models;

namespace MokoSnap.Core.Hotkeys;

public static class QuickSwitcherHotkeyDefaults
{
    private static readonly HotkeyGestureKey NewDefaultKey = new("M", true, true, false, false);
    private static readonly HotkeyGestureKey OldDefaultKey = new("Space", true, true, false, false);

    public static HotkeyGesture CreateDefault()
    {
        return new HotkeyGesture
        {
            Key = "M",
            Ctrl = true,
            Alt = true
        };
    }

    public static HotkeyGesture Resolve(HotkeyGesture? configuredHotkey)
    {
        if (ShouldUseDefault(configuredHotkey))
        {
            return CreateDefault();
        }

        return new HotkeyGesture
        {
            Key = HotkeyGestureFormatter.NormalizeKey(configuredHotkey!.Key),
            Ctrl = configuredHotkey.Ctrl,
            Alt = configuredHotkey.Alt,
            Shift = configuredHotkey.Shift,
            Windows = configuredHotkey.Windows
        };
    }

    public static bool ShouldUseDefault(HotkeyGesture? configuredHotkey)
    {
        if (configuredHotkey is null || string.IsNullOrWhiteSpace(configuredHotkey.Key))
        {
            return true;
        }

        HotkeyGestureKey key = HotkeyGestureKey.FromGesture(configuredHotkey);
        return key.Equals(OldDefaultKey) || key.Equals(default);
    }

    public static bool IsNewDefault(HotkeyGesture gesture)
    {
        return HotkeyGestureKey.FromGesture(gesture).Equals(NewDefaultKey);
    }
}
