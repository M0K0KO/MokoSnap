using MokoSnap.Core.ChromeCapture;
using MokoSnap.Platform.Windows.ChromeCapture;

namespace MokoSnap.App.Services;

public sealed class ChromeCaptureDiagnosticsService : IChromeCaptureDiagnosticsService
{
    private readonly ChromeNativeHostSetupService _setupService;

    public ChromeCaptureDiagnosticsService(ChromeNativeHostSetupService setupService)
    {
        _setupService = setupService;
    }

    public string GetStatusText()
    {
        return GetSnapshot().DetailsText;
    }

    public ChromeCaptureDiagnosticsSnapshot GetSnapshot()
    {
        try
        {
            ChromeNativeHostSetupStatus status = _setupService.CheckStatus(new ChromeNativeHostSetupRequest());
            string nativeHostLabel = status.Ready
                ? "OK"
                : status.Errors.Count > 0 ? "Error" : "Not configured";
            string nativeHostText = status.Ready
                ? "Native host is registered for the current user."
                : "Native host setup is incomplete. Open Chrome Capture Setup.";
            string captureLabel = status.LatestCaptureFileExists ? "OK" : "Not configured";
            string captureText = status.LatestCaptureFileExists
                ? $"Latest capture file found: {status.LatestCapturePath}"
                : $"No latest capture file yet. Expected file: {status.LatestCapturePath}";
            string issues = status.Errors.Count > 0
                ? $"Errors: {string.Join(" ", status.Errors)}"
                : status.Warnings.Count > 0 ? $"Warnings: {string.Join(" ", status.Warnings)}" : "No warnings.";
            return new ChromeCaptureDiagnosticsSnapshot
            {
                NativeHostStatusLabel = nativeHostLabel,
                NativeHostStatusText = nativeHostText,
                LatestCaptureStatusLabel = captureLabel,
                LatestCaptureStatusText = captureText,
                DetailsText =
                    $"Chrome native host: {nativeHostLabel} - {nativeHostText}{Environment.NewLine}" +
                    $"Chrome capture: {captureLabel} - {captureText}{Environment.NewLine}" +
                    issues
            };
        }
        catch (Exception ex)
        {
            return new ChromeCaptureDiagnosticsSnapshot
            {
                NativeHostStatusLabel = "Error",
                NativeHostStatusText = $"Chrome diagnostics failed: {ex.Message}",
                LatestCaptureStatusLabel = "Warning",
                LatestCaptureStatusText = "Latest capture status could not be checked.",
                DetailsText =
                    $"Chrome native host: Error - Chrome diagnostics failed: {ex.Message}{Environment.NewLine}" +
                    "Chrome capture: Warning - Latest capture status could not be checked."
            };
        }
    }
}
