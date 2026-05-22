using MokoSnap.Core.ChromeCapture;

namespace MokoSnap.App.Services;

public interface IChromeTabCaptureSelectionService
{
    Task<ChromeTabCaptureSelectionResult> SelectLatestCaptureAsync(CancellationToken cancellationToken = default);
}

public sealed class ChromeTabCaptureSelectionResult
{
    public IReadOnlyList<ChromeTabInfo> SelectedTabs { get; init; } = [];

    public string Message { get; init; } = string.Empty;

    public bool Succeeded => SelectedTabs.Count > 0;
}
