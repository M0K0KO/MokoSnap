namespace MokoSnap.Core.Models;

public sealed class WindowPlacementSnapshot
{
    public bool Enabled { get; set; } = true;

    public WindowPlacementShowState ShowState { get; set; } = WindowPlacementShowState.Normal;

    public int Left { get; set; }

    public int Top { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public string MonitorDeviceName { get; set; } = string.Empty;

    public bool WasProbablySnapped { get; set; }

    public WindowPlacementSnapshot Clone()
    {
        return new WindowPlacementSnapshot
        {
            Enabled = Enabled,
            ShowState = ShowState,
            Left = Left,
            Top = Top,
            Width = Width,
            Height = Height,
            MonitorDeviceName = MonitorDeviceName,
            WasProbablySnapped = WasProbablySnapped
        };
    }
}

public enum WindowPlacementShowState
{
    Normal,
    Maximized,
    Minimized
}
