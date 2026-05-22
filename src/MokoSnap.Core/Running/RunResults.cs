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

    public int ClosedWindowCount { get; set; }

    public string Message { get; set; } = string.Empty;
}
