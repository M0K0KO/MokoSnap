namespace MokoSnap.Core.Models;

public sealed class AppSettings
{
    public string Version { get; set; } = "1";

    public bool LaunchOnStartup { get; set; }

    public bool StartMinimizedToTray { get; set; }

    public bool MinimizeToTray { get; set; } = true;

    public HotkeyGesture? QuickSwitcherHotkey { get; set; }

    public List<Preset> Presets { get; set; } = [];

    public static AppSettings CreateDefault()
    {
        return new AppSettings();
    }
}
