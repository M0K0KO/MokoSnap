namespace MokoSnap.App.Services;

public interface IChromeCaptureDiagnosticsService
{
    ChromeCaptureDiagnosticsSnapshot GetSnapshot();

    string GetExtensionFolderPath();

    string GetStatusText();
}

public sealed class ChromeCaptureDiagnosticsSnapshot
{
    public string NativeHostStatusLabel { get; init; } = "Not configured";

    public string NativeHostStatusText { get; init; } = string.Empty;

    public string NativeHostExePath { get; init; } = string.Empty;

    public string LatestCaptureStatusLabel { get; init; } = "Not configured";

    public string LatestCaptureStatusText { get; init; } = string.Empty;

    public string LatestCapturePath { get; init; } = string.Empty;

    public string ExtensionSetupReminder { get; init; } = string.Empty;

    public string DetailsText { get; init; } = string.Empty;
}
