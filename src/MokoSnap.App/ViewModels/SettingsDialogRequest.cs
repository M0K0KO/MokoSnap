using MokoSnap.Core.Models;

namespace MokoSnap.App.ViewModels;

public sealed class SettingsDialogRequest
{
    public HotkeyGesture QuickSwitcherHotkey { get; init; } = new();

    public bool LaunchOnStartup { get; init; }

    public bool StartMinimizedToTray { get; init; }

    public bool MinimizeToTray { get; init; }

    public IReadOnlyList<Preset> Presets { get; init; } = [];

    public string? RegisteredStartupCommand { get; init; }

    public string ExpectedStartupCommand { get; init; } = string.Empty;

    public string StartupStatusError { get; init; } = string.Empty;

    public string HotkeyStatusText { get; init; } = string.Empty;

    public string ChromeCaptureStatusText { get; set; } = string.Empty;
}
