using MokoSnap.Core.Capture;

namespace MokoSnap.App.Services;

public interface ICapturedAppSelectionService
{
    Task<IReadOnlyList<CapturedWindowApp>> SelectCapturedAppsAsync(CancellationToken cancellationToken = default);
}
