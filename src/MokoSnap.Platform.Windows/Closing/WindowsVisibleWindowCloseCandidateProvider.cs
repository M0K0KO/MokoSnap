using MokoSnap.Core.Capture;
using MokoSnap.Core.Running;
using MokoSnap.Platform.Windows.Capture;

namespace MokoSnap.Platform.Windows.Closing;

public sealed class WindowsVisibleWindowCloseCandidateProvider : IVisibleWindowCloseCandidateProvider
{
    private readonly IVisibleAppCaptureService _captureService;

    public WindowsVisibleWindowCloseCandidateProvider(IVisibleAppCaptureService? captureService = null)
    {
        _captureService = captureService ?? new WindowsVisibleAppCaptureService();
    }

    public async Task<IReadOnlyList<CloseWindowCandidate>> GetCandidatesAsync(
        bool includeExplorer,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CapturedWindowApp> apps = await _captureService.CaptureVisibleAppsAsync(
            new VisibleAppCaptureOptions { IncludeExplorer = includeExplorer },
            cancellationToken);

        return apps
            .Select(app => new CloseWindowCandidate
            {
                WindowHandle = app.WindowHandle,
                WindowTitle = app.WindowTitle,
                ProcessName = app.ProcessName,
                ExecutablePath = app.ExecutablePath
            })
            .ToList();
    }
}
