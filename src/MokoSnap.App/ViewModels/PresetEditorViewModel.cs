using System.ComponentModel;
using System.Runtime.CompilerServices;
using MokoSnap.Core.Models;

namespace MokoSnap.App.ViewModels;

public sealed class PresetEditorViewModel : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _hotkeyText = string.Empty;
    private ClosePolicy _closePolicy;
    private CloseConfirmationPolicy _closeConfirmationPolicy = CloseConfirmationPolicy.AlwaysConfirm;

    public PresetEditorViewModel(Preset preset)
    {
        Id = preset.Id;
        Name = preset.Name;
        Description = preset.Description;
        HotkeyText = FormatHotkey(preset.Hotkey);
        ClosePolicy = preset.ClosePolicy;
        CloseConfirmationPolicy = preset.CloseConfirmationPolicy;
        Targets = preset.Targets;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; }

    public List<TargetConfig> Targets { get; }

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    public string HotkeyText
    {
        get => _hotkeyText;
        set => SetField(ref _hotkeyText, value);
    }

    public ClosePolicy ClosePolicy
    {
        get => _closePolicy;
        set => SetField(ref _closePolicy, value);
    }

    public CloseConfirmationPolicy CloseConfirmationPolicy
    {
        get => _closeConfirmationPolicy;
        set => SetField(ref _closeConfirmationPolicy, value);
    }

    public Preset ToPreset()
    {
        return new Preset
        {
            Id = Id,
            Name = Name.Trim(),
            Description = Description.Trim(),
            Hotkey = ParseHotkey(HotkeyText),
            ClosePolicy = ClosePolicy,
            CloseConfirmationPolicy = CloseConfirmationPolicy,
            Targets = Targets
        };
    }

    private static string FormatHotkey(HotkeyGesture? hotkey)
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

        parts.Add(hotkey.Key);
        return string.Join("+", parts);
    }

    private static HotkeyGesture? ParseHotkey(string value)
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
                hotkey.Key = part;
            }
        }

        return string.IsNullOrWhiteSpace(hotkey.Key) ? null : hotkey;
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
