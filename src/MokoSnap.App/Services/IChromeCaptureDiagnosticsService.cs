namespace MokoSnap.App.Services;

public interface IChromeCaptureDiagnosticsService
{
    ChromeCaptureDiagnosticsSnapshot GetSnapshot();

    string GetStatusText();
}

public sealed class ChromeCaptureDiagnosticsSnapshot
{
    public string NativeHostStatusLabel { get; init; } = "Not configured";

    public string NativeHostStatusText { get; init; } = string.Empty;

    public string LatestCaptureStatusLabel { get; init; } = "Not configured";

    public string LatestCaptureStatusText { get; init; } = string.Empty;

    public string DetailsText { get; init; } = string.Empty;
}
