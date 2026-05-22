using MokoSnap.Core.Models;

namespace MokoSnap.Core.Running;

public sealed class PresetRunResult
{
    public string PresetId { get; set; } = string.Empty;

    public string PresetName { get; set; } = string.Empty;

    public bool Succeeded { get; set; }

    public CloseWindowsResult? CloseWindowsResult { get; set; }

    public List<TargetRunResult> TargetResults { get; set; } = [];
}

public sealed class TargetRunResult
{
    public TargetConfig Target { get; set; } = new();

    public bool Succeeded { get; set; }

    public string Message { get; set; } = string.Empty;

    public static TargetRunResult Successful(TargetConfig target, string message = "")
    {
        return new TargetRunResult
        {
            Target = target,
            Succeeded = true,
            Message = message
        };
    }

    public static TargetRunResult Failed(TargetConfig target, string message)
    {
        return new TargetRunResult
        {
            Target = target,
            Succeeded = false,
            Message = message
        };
    }
}

public sealed class CloseWindowsResult
{
    public bool Succeeded { get; set; } = true;

    public bool Canceled { get; set; }

    public List<CloseWindowCandidate> CandidateWindows { get; set; } = [];

    public List<CloseWindowCandidate> ClosedWindows { get; set; } = [];

    public List<CloseWindowFailure> FailedWindows { get; set; } = [];

    public List<CloseWindowCandidate> SkippedWindows { get; set; } = [];

    public int ClosedWindowCount { get; set; }

    public string Message { get; set; } = string.Empty;
}

public sealed class CloseWindowsRequest
{
    public bool ConfirmBeforeClosing { get; set; }

    public bool IncludeExplorer { get; set; }
}

public sealed class CloseWindowCandidate
{
    public long WindowHandle { get; set; }

    public string WindowTitle { get; set; } = string.Empty;

    public string ProcessName { get; set; } = string.Empty;

    public string ExecutablePath { get; set; } = string.Empty;
}

public sealed class CloseWindowFailure
{
    public CloseWindowCandidate Window { get; set; } = new();

    public string Message { get; set; } = string.Empty;
}

public sealed class CloseWindowsSelectionResult
{
    public bool Canceled { get; set; }

    public List<long> SelectedWindowHandles { get; set; } = [];
}
