using MokoSnap.App.Views;
using MokoSnap.Core.Capture;

namespace MokoSnap.App.Services;

public sealed class CapturedAppSelectionService : ICapturedAppSelectionService
{
    private readonly IVisibleAppCaptureService _captureService;

    public CapturedAppSelectionService(IVisibleAppCaptureService captureService)
    {
        _captureService = captureService;
    }

    public Task<IReadOnlyList<CapturedWindowApp>> SelectCapturedAppsAsync(CancellationToken cancellationToken = default)
    {
        CapturedAppsDialog dialog = new(_captureService);
        bool? result = dialog.ShowDialog();
        IReadOnlyList<CapturedWindowApp> selectedApps = result == true
            ? dialog.SelectedApps
            : [];

        return Task.FromResult(selectedApps);
    }
}
