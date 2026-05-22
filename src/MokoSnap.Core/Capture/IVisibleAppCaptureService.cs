namespace MokoSnap.Core.Capture;

public interface IVisibleAppCaptureService
{
    Task<IReadOnlyList<CapturedWindowApp>> CaptureVisibleAppsAsync(
        VisibleAppCaptureOptions options,
        CancellationToken cancellationToken = default);
}
