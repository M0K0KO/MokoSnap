using MokoSnap.Core.Models;

namespace MokoSnap.Core.Hotkeys;

public static class HotkeyValidator
{
    private static readonly HashSet<string> CommonCtrlShortcuts = new(StringComparer.OrdinalIgnoreCase)
    {
        "A",
        "C",
        "P",
        "S",
        "V",
        "X",
        "Z"
    };

    public static HotkeyValidationResult ValidatePresets(IEnumerable<Preset> presets)
    {
        List<HotkeyValidationMessage> messages = [];
        Dictionary<HotkeyGestureKey, Preset> seen = [];

        foreach (Preset preset in presets)
        {
            HotkeyGesture? gesture = preset.Hotkey;
            if (gesture is null || string.IsNullOrWhiteSpace(gesture.Key))
            {
                continue;
            }

            messages.AddRange(ValidateGesture(gesture, preset.Id, preset.Name).Messages);

            HotkeyGestureKey key = HotkeyGestureKey.FromGesture(gesture);
            if (seen.TryGetValue(key, out Preset? conflictingPreset))
            {
                messages.Add(HotkeyValidationMessage.Error(
                    preset.Id,
                    preset.Name,
                    gesture,
                    $"Hotkey {key} is already used by {DisplayName(conflictingPreset)}.",
                    DisplayName(conflictingPreset)));
            }
            else
            {
                seen[key] = preset;
            }
        }

        return new HotkeyValidationResult(messages);
    }

    public static HotkeyValidationResult ValidateGesture(
        HotkeyGesture? gesture,
        string presetId = "",
        string presetName = "")
    {
        if (gesture is null || string.IsNullOrWhiteSpace(gesture.Key))
        {
            return new HotkeyValidationResult([]);
        }

        List<HotkeyValidationMessage> messages = [];
        string normalizedKey = HotkeyGestureFormatter.NormalizeKey(gesture.Key);
        bool hasModifier = gesture.Ctrl || gesture.Alt || gesture.Shift || gesture.Windows;

        if (!hasModifier)
        {
            messages.Add(HotkeyValidationMessage.Error(
                presetId,
                presetName,
                gesture,
                "Hotkey must include Ctrl, Alt, Shift, or Win."));
        }

        if (gesture.Alt &&
            !gesture.Ctrl &&
            !gesture.Shift &&
            !gesture.Windows &&
            normalizedKey.Equals("F4", StringComparison.OrdinalIgnoreCase))
        {
            messages.Add(HotkeyValidationMessage.Error(
                presetId,
                presetName,
                gesture,
                "Alt+F4 is reserved by Windows."));
        }

        if (gesture.Ctrl &&
            !gesture.Alt &&
            !gesture.Shift &&
            !gesture.Windows &&
            CommonCtrlShortcuts.Contains(normalizedKey))
        {
            messages.Add(HotkeyValidationMessage.Warning(
                presetId,
                presetName,
                gesture,
                $"Ctrl+{normalizedKey} is a common application shortcut."));
        }

        return new HotkeyValidationResult(messages);
    }

    private static string DisplayName(Preset preset)
    {
        return string.IsNullOrWhiteSpace(preset.Name) ? "unnamed preset" : preset.Name;
    }
}

public sealed class HotkeyValidationResult
{
    public HotkeyValidationResult(IReadOnlyList<HotkeyValidationMessage> messages)
    {
        Messages = messages;
    }

    public IReadOnlyList<HotkeyValidationMessage> Messages { get; }

    public bool IsValid => Messages.All(message => message.Severity != HotkeyValidationSeverity.Error);

    public IReadOnlyList<HotkeyValidationMessage> Errors =>
        Messages.Where(message => message.Severity == HotkeyValidationSeverity.Error).ToList();

    public IReadOnlyList<HotkeyValidationMessage> Warnings =>
        Messages.Where(message => message.Severity == HotkeyValidationSeverity.Warning).ToList();
}

public sealed class HotkeyValidationMessage
{
    private HotkeyValidationMessage(
        HotkeyValidationSeverity severity,
        string presetId,
        string presetName,
        HotkeyGesture? gesture,
        string message,
        string? conflictingPresetName)
    {
        Severity = severity;
        PresetId = presetId;
        PresetName = presetName;
        Gesture = gesture;
        Message = message;
        ConflictingPresetName = conflictingPresetName;
    }

    public HotkeyValidationSeverity Severity { get; }

    public string PresetId { get; }

    public string PresetName { get; }

    public HotkeyGesture? Gesture { get; }

    public string Message { get; }

    public string? ConflictingPresetName { get; }

    public static HotkeyValidationMessage Error(
        string presetId,
        string presetName,
        HotkeyGesture? gesture,
        string message,
        string? conflictingPresetName = null)
    {
        return new HotkeyValidationMessage(
            HotkeyValidationSeverity.Error,
            presetId,
            presetName,
            gesture,
            message,
            conflictingPresetName);
    }

    public static HotkeyValidationMessage Warning(
        string presetId,
        string presetName,
        HotkeyGesture? gesture,
        string message)
    {
        return new HotkeyValidationMessage(
            HotkeyValidationSeverity.Warning,
            presetId,
            presetName,
            gesture,
            message,
            null);
    }
}

public enum HotkeyValidationSeverity
{
    Error,
    Warning
}
