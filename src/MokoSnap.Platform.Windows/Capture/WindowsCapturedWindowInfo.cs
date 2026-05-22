using MokoSnap.Core.Models;

namespace MokoSnap.Platform.Windows.Capture;

public sealed class WindowsCapturedWindowInfo
{
    public string WindowTitle { get; set; } = string.Empty;

    public string ProcessName { get; set; } = string.Empty;

    public string ExecutablePath { get; set; } = string.Empty;

    public int ProcessId { get; set; }

    public long WindowHandle { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public WindowPlacementSnapshot? WindowPlacement { get; set; }
}
