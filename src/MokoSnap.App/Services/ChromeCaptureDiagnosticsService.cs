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
        try
        {
            ChromeNativeHostSetupStatus status = _setupService.CheckStatus(new ChromeNativeHostSetupRequest());
            string nativeHost = status.ManifestFileExists &&
                status.ManifestJsonValid &&
                status.RegistryKeyExists &&
                status.RegistryValuePointsToExpectedManifest
                    ? "configured"
                    : "not configured";
            string capture = status.LatestCaptureFileExists
                ? $"latest capture found: {status.LatestCapturePath}"
                : $"latest capture missing: {status.LatestCapturePath}";
            string issues = status.Errors.Count > 0
                ? $"Errors: {string.Join(" ", status.Errors)}"
                : status.Warnings.Count > 0 ? $"Warnings: {string.Join(" ", status.Warnings)}" : "No warnings.";
            return $"Chrome native host: {nativeHost}{Environment.NewLine}Chrome capture: {capture}{Environment.NewLine}{issues}";
        }
        catch (Exception ex)
        {
            return $"Chrome native host: warning/error{Environment.NewLine}Chrome diagnostics failed: {ex.Message}";
        }
    }
}
