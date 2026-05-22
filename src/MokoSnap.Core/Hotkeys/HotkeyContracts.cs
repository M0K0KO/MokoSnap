using MokoSnap.Core.Models;

namespace MokoSnap.Core.Hotkeys;

public interface IHotkeyService : IDisposable
{
    event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

    HotkeyRegistrationResult RegisterCommandPaletteHotkey(HotkeyGesture gesture);

    HotkeyRegistrationResult RegisterPresetHotkey(string presetId, string presetName, HotkeyGesture gesture);

    void UnregisterPresetHotkey(string presetId);

    void UnregisterAll();
}

public sealed class HotkeyPressedEventArgs : EventArgs
{
    public bool IsCommandPalette { get; init; }

    public string? PresetId { get; init; }
}

public sealed class HotkeyRegistrationResult
{
    public bool Success { get; init; }

    public HotkeyGesture? FailedGesture { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public string? ConflictingPresetName { get; init; }

    public static HotkeyRegistrationResult Succeeded()
    {
        return new HotkeyRegistrationResult { Success = true };
    }

    public static HotkeyRegistrationResult Failed(
        HotkeyGesture gesture,
        string errorMessage,
        string? conflictingPresetName = null)
    {
        return new HotkeyRegistrationResult
        {
            Success = false,
            FailedGesture = gesture,
            ErrorMessage = errorMessage,
            ConflictingPresetName = conflictingPresetName
        };
    }
}
