using MokoSnap.Core.Models;

namespace MokoSnap.Platform.Windows.Placement;

public interface IWindowsWindowPlacementService
{
    WindowPlacementSnapshot? CapturePlacement(IntPtr windowHandle);

    Task<WindowPlacementRestoreResult> RestorePlacementAsync(
        TargetConfig target,
        int? launchedProcessId,
        CancellationToken cancellationToken = default);
}
