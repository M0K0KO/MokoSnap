using MokoSnap.Core.Models;

namespace MokoSnap.App.ViewModels;

public sealed class SettingsDialogResult
{
    public HotkeyGesture QuickSwitcherHotkey { get; init; } = new();

    public bool LaunchOnStartup { get; init; }

    public bool StartMinimizedToTray { get; init; }

    public bool MinimizeToTray { get; init; }
}
