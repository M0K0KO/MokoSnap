namespace MokoSnap.Platform.Windows.Placement;

public sealed class WindowPlacementRestoreResult
{
    public bool Succeeded { get; set; }

    public string Message { get; set; } = string.Empty;

    public static WindowPlacementRestoreResult Successful()
    {
        return new WindowPlacementRestoreResult { Succeeded = true };
    }

    public static WindowPlacementRestoreResult Failed(string message)
    {
        return new WindowPlacementRestoreResult
        {
            Succeeded = false,
            Message = message
        };
    }
}
