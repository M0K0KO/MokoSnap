namespace MokoSnap.Core.Models;

public sealed class Preset
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public HotkeyGesture? Hotkey { get; set; }

    public ClosePolicy ClosePolicy { get; set; } = ClosePolicy.None;

    public CloseConfirmationPolicy CloseConfirmationPolicy { get; set; } = CloseConfirmationPolicy.AlwaysConfirm;

    public List<TargetConfig> Targets { get; set; } = [];
}

public sealed class TargetConfig
{
    public TargetType Type { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string ExecutablePath { get; set; } = string.Empty;

    public string Arguments { get; set; } = string.Empty;

    public string WorkingDirectory { get; set; } = string.Empty;

    public int LaunchDelayMs { get; set; }

    public bool RunAsAdmin { get; set; }

    public WindowPlacementSnapshot? WindowPlacement { get; set; }

    public string ProfileName { get; set; } = string.Empty;

    public bool OpenInNewWindow { get; set; }

    public List<string> Urls { get; set; } = [];

    public List<string> PageUrls { get; set; } = [];

    public bool PreferDesktopApp { get; set; } = true;

    public string Url { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;
}

public enum TargetType
{
    Application,
    Chrome,
    Notion,
    Url,
    Folder
}

public enum ClosePolicy
{
    None,
    CloseVisibleWindowsOnly
}

public enum CloseConfirmationPolicy
{
    AlwaysConfirm,
    SkipConfirmation
}

public sealed class HotkeyGesture
{
    public string Key { get; set; } = string.Empty;

    public bool Ctrl { get; set; }

    public bool Alt { get; set; }

    public bool Shift { get; set; }

    public bool Windows { get; set; }
}
