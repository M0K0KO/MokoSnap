using MokoSnap.Core.Capture;

namespace MokoSnap.Platform.Windows.Capture;

public sealed class WindowsVisibleAppFilter
{
    private static readonly HashSet<string> SystemClassNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Progman",
        "WorkerW",
        "Shell_TrayWnd",
        "Shell_SecondaryTrayWnd"
    };

    private static readonly HashSet<string> SystemProcessNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "SearchHost",
        "ShellExperienceHost",
        "StartMenuExperienceHost",
        "TextInputHost"
    };

    public bool ShouldInclude(WindowsCapturedWindowInfo window, VisibleAppCaptureOptions options)
    {
        if (string.IsNullOrWhiteSpace(window.WindowTitle))
        {
            return false;
        }

        if (window.ProcessId == Environment.ProcessId || IsMokoSnapProcess(window.ProcessName))
        {
            return false;
        }

        if (SystemClassNames.Contains(window.ClassName) || SystemProcessNames.Contains(window.ProcessName))
        {
            return false;
        }

        if (IsExplorer(window.ProcessName) && !options.IncludeExplorer)
        {
            return false;
        }

        return true;
    }

    public CapturedWindowApp ToCapturedApp(WindowsCapturedWindowInfo window)
    {
        return new CapturedWindowApp
        {
            WindowTitle = window.WindowTitle,
            ProcessName = window.ProcessName,
            ExecutablePath = window.ExecutablePath,
            ProcessId = window.ProcessId,
            WindowHandle = window.WindowHandle,
            IsExplorer = IsExplorer(window.ProcessName),
            WindowPlacement = window.WindowPlacement?.Clone()
        };
    }

    private static bool IsMokoSnapProcess(string processName)
    {
        return processName.StartsWith("MokoSnap", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsExplorer(string processName)
    {
        return processName.Equals("explorer", StringComparison.OrdinalIgnoreCase);
    }
}
