using System.IO;
using System.Text.Json;
using MokoSnap.Core.ChromeCapture;
using MokoSnap.Core.Storage;
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

    public string GetExtensionFolderPath()
    {
        return _setupService.ExtensionFolderPath;
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
            string captureText = BuildLatestCaptureStatusText(status, ref captureLabel);
            string issues = status.Errors.Count > 0
                ? $"Errors: {string.Join(" ", status.Errors)}"
                : status.Warnings.Count > 0 ? $"Warnings: {string.Join(" ", status.Warnings)}" : "No warnings.";
            return new ChromeCaptureDiagnosticsSnapshot
            {
                NativeHostStatusLabel = nativeHostLabel,
                NativeHostStatusText = nativeHostText,
                NativeHostExePath = status.NativeHostExePath,
                LatestCaptureStatusLabel = captureLabel,
                LatestCaptureStatusText = captureText,
                LatestCapturePath = status.LatestCapturePath,
                ExtensionSetupReminder = "Load the unpacked extension manually, paste its extension ID in Chrome Capture Setup, register the Native Host, then restart Chrome if capture fails.",
                DetailsText =
                    $"Chrome native host: {nativeHostLabel} - {nativeHostText}{Environment.NewLine}" +
                    $"Native host exe: {status.NativeHostExePath}{Environment.NewLine}" +
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
                ExtensionSetupReminder = "Load the unpacked extension manually, paste its extension ID in Chrome Capture Setup, register the Native Host, then restart Chrome if capture fails.",
                DetailsText =
                    $"Chrome native host: Error - Chrome diagnostics failed: {ex.Message}{Environment.NewLine}" +
                    "Chrome capture: Warning - Latest capture status could not be checked."
            };
        }
    }

    private static string BuildLatestCaptureStatusText(ChromeNativeHostSetupStatus status, ref string captureLabel)
    {
        if (!status.LatestCaptureFileExists)
        {
            return $"No latest capture file yet. Capture tabs from the Chrome extension first. Expected file: {status.LatestCapturePath}";
        }

        try
        {
            string json = File.ReadAllText(status.LatestCapturePath);
            ChromeTabCapture? capture = JsonSerializer.Deserialize<ChromeTabCapture>(
                json,
                FileJsonStorage<ChromeTabCapture>.CreateJsonSerializerOptions());
            if (capture is null)
            {
                captureLabel = "Error";
                return $"Latest capture file is empty or invalid JSON. Capture tabs again. File: {status.LatestCapturePath}";
            }

            int windowCount = capture.Windows.Count;
            int tabCount = capture.Tabs.Count > 0
                ? capture.Tabs.Count
                : capture.Windows.Sum(window => window.Tabs.Count);
            return $"Latest capture file found: {status.LatestCapturePath}. Captured at {capture.CapturedAt.LocalDateTime:g}; {windowCount} window(s), {tabCount} tab(s).";
        }
        catch (JsonException ex)
        {
            captureLabel = "Error";
            return $"Latest capture file is invalid JSON. Capture tabs again from the extension. File: {status.LatestCapturePath}. {ex.Message}";
        }
        catch (IOException ex)
        {
            captureLabel = "Error";
            return $"Latest capture file could not be read. File: {status.LatestCapturePath}. {ex.Message}";
        }
    }
}
