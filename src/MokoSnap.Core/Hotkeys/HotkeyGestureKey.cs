using MokoSnap.Core.Models;

namespace MokoSnap.Core.Hotkeys;

public readonly record struct HotkeyGestureKey(
    string Key,
    bool Ctrl,
    bool Alt,
    bool Shift,
    bool Windows)
{
    public static HotkeyGestureKey FromGesture(HotkeyGesture gesture)
    {
        return new HotkeyGestureKey(
            HotkeyGestureFormatter.NormalizeKey(gesture.Key),
            gesture.Ctrl,
            gesture.Alt,
            gesture.Shift,
            gesture.Windows);
    }

    public override string ToString()
    {
        return HotkeyGestureFormatter.Format(new HotkeyGesture
        {
            Key = Key,
            Ctrl = Ctrl,
            Alt = Alt,
            Shift = Shift,
            Windows = Windows
        });
    }
}
