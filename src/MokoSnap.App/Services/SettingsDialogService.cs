using System.Windows;
using MokoSnap.App.ViewModels;
using MokoSnap.App.Views;
using MokoSnap.Core.ChromeCapture;
using MokoSnap.Platform.Windows.ChromeCapture;

namespace MokoSnap.App.Services;

public sealed class SettingsDialogService : ISettingsDialogService
{
    private readonly Window _owner;
    private readonly IChromeNativeHostSetupDialogService _chromeSetupDialogService;
    private readonly ChromeNativeHostSetupService _chromeSetupService;

    public SettingsDialogService(
        Window owner,
        IChromeNativeHostSetupDialogService chromeSetupDialogService,
        ChromeNativeHostSetupService chromeSetupService)
    {
        _owner = owner;
        _chromeSetupDialogService = chromeSetupDialogService;
        _chromeSetupService = chromeSetupService;
    }

    public SettingsDialogResult? ShowSettings(SettingsDialogRequest request)
    {
        ShowOwnerWindow();
        request.ChromeCaptureStatusText = BuildChromeCaptureStatusText();
        SettingsDialogViewModel viewModel = new(request, _chromeSetupDialogService.ShowSetupDialog);
        SettingsDialog dialog = new()
        {
            Owner = _owner,
            DataContext = viewModel
        };

        bool? result = dialog.ShowDialog();
        return result == true ? dialog.Result : null;
    }

    private string BuildChromeCaptureStatusText()
    {
        try
        {
            ChromeNativeHostSetupStatus status = _chromeSetupService.CheckStatus(new ChromeNativeHostSetupRequest());
            string latestCapture = status.LatestCaptureFileExists
                ? $"exists: {status.LatestCapturePath}"
                : $"missing. Capture tabs from the Chrome extension first. Expected file: {status.LatestCapturePath}";
            string nativeHost = status.ManifestFileExists &&
                status.ManifestJsonValid &&
                status.RegistryKeyExists &&
                status.RegistryValuePointsToExpectedManifest
                    ? "configured"
                    : "not fully configured";
            return $"Latest capture file: {latestCapture}{Environment.NewLine}Native host registration: {nativeHost}";
        }
        catch (Exception ex)
        {
            return $"Chrome Capture status error: {ex.Message}";
        }
    }

    private void ShowOwnerWindow()
    {
        _owner.Show();
        if (_owner.WindowState == WindowState.Minimized)
        {
            _owner.WindowState = WindowState.Normal;
        }

        DialogFocusHelper.ActivateAndFocus(_owner, _owner);
    }
}
