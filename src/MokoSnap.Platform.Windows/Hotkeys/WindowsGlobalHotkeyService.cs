using System.ComponentModel;
using System.Runtime.InteropServices;
using MokoSnap.Core.Hotkeys;
using MokoSnap.Core.Models;

namespace MokoSnap.Platform.Windows.Hotkeys;

public sealed class WindowsGlobalHotkeyService : IHotkeyService
{
    public const int WmHotkey = 0x0312;
    private const int ModAlt = 0x0001;
    private const int ModControl = 0x0002;
    private const int ModShift = 0x0004;
    private const int ModWin = 0x0008;
    private const int ModNoRepeat = 0x4000;
    private const string CommandPaletteKey = "__command_palette";

    private readonly IntPtr _windowHandle;
    private readonly Dictionary<string, RegisteredHotkey> _registeredByKey = [];
    private readonly Dictionary<int, RegisteredHotkey> _registeredById = [];
    private int _nextId = 0x4D00;
    private bool _disposed;

    public WindowsGlobalHotkeyService(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;
    }

    public event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

    public HotkeyRegistrationResult RegisterCommandPaletteHotkey(HotkeyGesture gesture)
    {
        return Register(CommandPaletteKey, null, "Command palette", gesture, true);
    }

    public HotkeyRegistrationResult RegisterPresetHotkey(string presetId, string presetName, HotkeyGesture gesture)
    {
        if (string.IsNullOrWhiteSpace(presetId))
        {
            return HotkeyRegistrationResult.Failed(gesture, "Preset id is required.");
        }

        return Register(presetId, presetId, presetName, gesture, false);
    }

    public void UnregisterPresetHotkey(string presetId)
    {
        Unregister(presetId);
    }

    public void UnregisterAll()
    {
        foreach (RegisteredHotkey hotkey in _registeredByKey.Values.ToList())
        {
            UnregisterHotKey(_windowHandle, hotkey.Id);
        }

        _registeredByKey.Clear();
        _registeredById.Clear();
    }

    public bool ProcessWindowMessage(int message, IntPtr wParam)
    {
        if (message != WmHotkey)
        {
            return false;
        }

        int id = wParam.ToInt32();
        if (!_registeredById.TryGetValue(id, out RegisteredHotkey? hotkey))
        {
            return false;
        }

        HotkeyPressed?.Invoke(this, new HotkeyPressedEventArgs
        {
            IsCommandPalette = hotkey.IsCommandPalette,
            PresetId = hotkey.PresetId
        });
        return true;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        UnregisterAll();
        _disposed = true;
    }

    private HotkeyRegistrationResult Register(
        string registrationKey,
        string? presetId,
        string displayName,
        HotkeyGesture gesture,
        bool isCommandPalette)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!TryGetVirtualKey(gesture.Key, out int virtualKey))
        {
            return HotkeyRegistrationResult.Failed(gesture, $"Unsupported hotkey key '{gesture.Key}'.");
        }

        HotkeyGestureKey gestureKey = HotkeyGestureKey.FromGesture(gesture);
        RegisteredHotkey? conflict = _registeredByKey.Values.FirstOrDefault(existing =>
            existing.RegistrationKey != registrationKey && existing.GestureKey == gestureKey);
        if (conflict is not null)
        {
            return HotkeyRegistrationResult.Failed(
                gesture,
                $"Hotkey {gestureKey} is already registered.",
                conflict.IsCommandPalette ? "Command palette" : conflict.DisplayName);
        }

        Unregister(registrationKey);

        int modifiers = GetModifiers(gesture);
        int id = _nextId++;
        if (!RegisterHotKey(_windowHandle, id, modifiers, virtualKey))
        {
            string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
            return HotkeyRegistrationResult.Failed(gesture, errorMessage);
        }

        RegisteredHotkey hotkey = new(
            id,
            registrationKey,
            presetId,
            displayName,
            gestureKey,
            isCommandPalette);
        _registeredByKey[registrationKey] = hotkey;
        _registeredById[id] = hotkey;
        return HotkeyRegistrationResult.Succeeded();
    }

    private void Unregister(string registrationKey)
    {
        if (!_registeredByKey.Remove(registrationKey, out RegisteredHotkey? hotkey))
        {
            return;
        }

        _registeredById.Remove(hotkey.Id);
        UnregisterHotKey(_windowHandle, hotkey.Id);
    }

    private static int GetModifiers(HotkeyGesture gesture)
    {
        int modifiers = ModNoRepeat;
        if (gesture.Ctrl)
        {
            modifiers |= ModControl;
        }

        if (gesture.Alt)
        {
            modifiers |= ModAlt;
        }

        if (gesture.Shift)
        {
            modifiers |= ModShift;
        }

        if (gesture.Windows)
        {
            modifiers |= ModWin;
        }

        return modifiers;
    }

    private static bool TryGetVirtualKey(string key, out int virtualKey)
    {
        string normalizedKey = HotkeyGestureFormatter.NormalizeKey(key);
        if (normalizedKey.Length == 1)
        {
            char character = normalizedKey[0];
            if (character is >= 'A' and <= 'Z' or >= '0' and <= '9')
            {
                virtualKey = character;
                return true;
            }
        }

        if (normalizedKey.StartsWith("F", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(normalizedKey[1..], out int functionNumber) &&
            functionNumber is >= 1 and <= 24)
        {
            virtualKey = 0x70 + functionNumber - 1;
            return true;
        }

        return NamedVirtualKeys.TryGetValue(normalizedKey, out virtualKey);
    }

    private static readonly Dictionary<string, int> NamedVirtualKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Backspace"] = 0x08,
        ["Tab"] = 0x09,
        ["Enter"] = 0x0D,
        ["Escape"] = 0x1B,
        ["Space"] = 0x20,
        ["PageUp"] = 0x21,
        ["PageDown"] = 0x22,
        ["End"] = 0x23,
        ["Home"] = 0x24,
        ["Left"] = 0x25,
        ["Up"] = 0x26,
        ["Right"] = 0x27,
        ["Down"] = 0x28,
        ["Insert"] = 0x2D,
        ["Delete"] = 0x2E
    };

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr windowHandle, int id, int modifiers, int virtualKey);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr windowHandle, int id);

    private sealed record RegisteredHotkey(
        int Id,
        string RegistrationKey,
        string? PresetId,
        string DisplayName,
        HotkeyGestureKey GestureKey,
        bool IsCommandPalette);
}
