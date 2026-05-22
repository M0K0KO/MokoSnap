namespace MokoSnap.Core.Models;

public sealed class LaunchHistory
{
    public List<LaunchHistoryEntry> Entries { get; set; } = [];

    public static LaunchHistory CreateDefault()
    {
        return new LaunchHistory();
    }
}

public sealed class LaunchHistoryEntry
{
    public string PresetId { get; set; } = string.Empty;

    public DateTimeOffset LaunchedAt { get; set; } = DateTimeOffset.UtcNow;

    public bool Succeeded { get; set; }

    public string Message { get; set; } = string.Empty;
}
