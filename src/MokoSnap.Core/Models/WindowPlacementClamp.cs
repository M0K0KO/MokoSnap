namespace MokoSnap.Core.Models;

public static class WindowPlacementClamp
{
    public static WindowPlacementSnapshot ClampToVisibleWorkArea(
        WindowPlacementSnapshot snapshot,
        IReadOnlyList<WindowPlacementWorkArea> workAreas)
    {
        if (workAreas.Count == 0)
        {
            return snapshot.Clone();
        }

        WindowPlacementSnapshot result = snapshot.Clone();
        WindowPlacementWorkArea workArea = FindWorkArea(result, workAreas);
        int width = Math.Clamp(result.Width, 1, Math.Max(1, workArea.Width));
        int height = Math.Clamp(result.Height, 1, Math.Max(1, workArea.Height));

        result.Width = width;
        result.Height = height;
        result.Left = Math.Clamp(result.Left, workArea.Left, Math.Max(workArea.Left, workArea.Right - width));
        result.Top = Math.Clamp(result.Top, workArea.Top, Math.Max(workArea.Top, workArea.Bottom - height));
        result.MonitorDeviceName = workArea.DeviceName;

        return result;
    }

    private static WindowPlacementWorkArea FindWorkArea(
        WindowPlacementSnapshot snapshot,
        IReadOnlyList<WindowPlacementWorkArea> workAreas)
    {
        if (!string.IsNullOrWhiteSpace(snapshot.MonitorDeviceName))
        {
            WindowPlacementWorkArea? namedArea = workAreas.FirstOrDefault(workArea =>
                workArea.DeviceName.Equals(snapshot.MonitorDeviceName, StringComparison.OrdinalIgnoreCase));
            if (namedArea is not null)
            {
                return namedArea;
            }
        }

        WindowPlacementWorkArea? intersectingArea = workAreas.FirstOrDefault(workArea =>
            snapshot.Left < workArea.Right &&
            snapshot.Left + snapshot.Width > workArea.Left &&
            snapshot.Top < workArea.Bottom &&
            snapshot.Top + snapshot.Height > workArea.Top);

        return intersectingArea ??
            workAreas.FirstOrDefault(workArea => workArea.IsPrimary) ??
            workAreas[0];
    }
}
