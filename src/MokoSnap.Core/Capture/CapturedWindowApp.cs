using MokoSnap.Core.Models;

namespace MokoSnap.Core.Capture;

public sealed class CapturedWindowApp
{
    public string WindowTitle { get; set; } = string.Empty;

    public string ProcessName { get; set; } = string.Empty;

    public string ExecutablePath { get; set; } = string.Empty;

    public int ProcessId { get; set; }

    public long WindowHandle { get; set; }

    public bool IsExplorer { get; set; }

    public WindowPlacementSnapshot? WindowPlacement { get; set; }

    public bool IsAvailable => !string.IsNullOrWhiteSpace(ExecutablePath);

    public TargetConfig ToApplicationTarget()
    {
        return new TargetConfig
        {
            Type = TargetType.Application,
            DisplayName = GetFriendlyName(),
            ExecutablePath = ExecutablePath,
            Arguments = string.Empty,
            WorkingDirectory = string.Empty,
            LaunchDelayMs = 0,
            RunAsAdmin = false,
            WindowPlacement = WindowPlacement?.Clone()
        };
    }

    private string GetFriendlyName()
    {
        if (!string.IsNullOrWhiteSpace(ProcessName) && !string.IsNullOrWhiteSpace(WindowTitle))
        {
            return $"{ProcessName} - {WindowTitle}";
        }

        return string.IsNullOrWhiteSpace(WindowTitle) ? ProcessName : WindowTitle;
    }
}
