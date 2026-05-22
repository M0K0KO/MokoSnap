using MokoSnap.Core.Models;

namespace MokoSnap.Core.Hotkeys;

public static class HotkeyGestureFormatter
{
    public static string Format(HotkeyGesture? hotkey)
    {
        if (hotkey is null || string.IsNullOrWhiteSpace(hotkey.Key))
        {
            return string.Empty;
        }

        List<string> parts = [];
        if (hotkey.Ctrl)
        {
            parts.Add("Ctrl");
        }

        if (hotkey.Alt)
        {
            parts.Add("Alt");
        }

        if (hotkey.Shift)
        {
            parts.Add("Shift");
        }

        if (hotkey.Windows)
        {
            parts.Add("Win");
        }

        parts.Add(NormalizeKey(hotkey.Key));
        return string.Join("+", parts);
    }

    public static HotkeyGesture? Parse(string value)
    {
        string[] parts = value.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return null;
        }

        HotkeyGesture hotkey = new();
        foreach (string part in parts)
        {
            if (part.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) ||
                part.Equals("Control", StringComparison.OrdinalIgnoreCase))
            {
                hotkey.Ctrl = true;
            }
            else if (part.Equals("Alt", StringComparison.OrdinalIgnoreCase))
            {
                hotkey.Alt = true;
            }
            else if (part.Equals("Shift", StringComparison.OrdinalIgnoreCase))
            {
                hotkey.Shift = true;
            }
            else if (part.Equals("Win", StringComparison.OrdinalIgnoreCase) ||
                     part.Equals("Windows", StringComparison.OrdinalIgnoreCase))
            {
                hotkey.Windows = true;
            }
            else
            {
                hotkey.Key = NormalizeKey(part);
            }
        }

        return string.IsNullOrWhiteSpace(hotkey.Key) ? null : hotkey;
    }

    public static string NormalizeKey(string key)
    {
        string trimmed = key.Trim();
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        if (trimmed.Length == 1)
        {
            return trimmed.ToUpperInvariant();
        }

        return trimmed.Equals("Esc", StringComparison.OrdinalIgnoreCase)
            ? "Escape"
            : char.ToUpperInvariant(trimmed[0]) + trimmed[1..];
    }
}
