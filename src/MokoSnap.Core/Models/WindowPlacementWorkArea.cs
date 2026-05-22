namespace MokoSnap.Core.Models;

public sealed class WindowPlacementWorkArea
{
    public string DeviceName { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public int Left { get; set; }

    public int Top { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public int Right => Left + Width;

    public int Bottom => Top + Height;
}
