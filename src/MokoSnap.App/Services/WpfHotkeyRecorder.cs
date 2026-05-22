using System.Windows.Input;

namespace MokoSnap.App.Services;

public static class WpfHotkeyRecorder
{
    public static string FormatRecordedHotkey(Key key, ModifierKeys modifiers)
    {
        List<string> parts = [];
        if (modifiers.HasFlag(ModifierKeys.Control))
        {
            parts.Add("Ctrl");
        }

        if (modifiers.HasFlag(ModifierKeys.Alt))
        {
            parts.Add("Alt");
        }

        if (modifiers.HasFlag(ModifierKeys.Shift))
        {
            parts.Add("Shift");
        }

        if (modifiers.HasFlag(ModifierKeys.Windows))
        {
            parts.Add("Win");
        }

        parts.Add(FormatRecordedKey(key));
        return string.Join("+", parts);
    }

    public static bool IsModifierKey(Key key)
    {
        return key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or
            Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin;
    }

    private static string FormatRecordedKey(Key key)
    {
        return key switch
        {
            Key.Back => "Backspace",
            Key.Return => "Enter",
            Key.Escape => "Escape",
            Key.Space => "Space",
            Key.Prior => "PageUp",
            Key.Next => "PageDown",
            Key.Delete => "Delete",
            Key.Insert => "Insert",
            Key.D0 => "0",
            Key.D1 => "1",
            Key.D2 => "2",
            Key.D3 => "3",
            Key.D4 => "4",
            Key.D5 => "5",
            Key.D6 => "6",
            Key.D7 => "7",
            Key.D8 => "8",
            Key.D9 => "9",
            Key.Left => "Left",
            Key.Up => "Up",
            Key.Right => "Right",
            Key.Down => "Down",
            _ => key.ToString()
        };
    }
}
