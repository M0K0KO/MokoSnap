using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using MokoSnap.Core.ChromeCapture;
using MokoSnap.Platform.Windows.ChromeCapture;

namespace MokoSnap.App.ViewModels;

public sealed class ChromeNativeHostSetupDialogViewModel : INotifyPropertyChanged
{
    private readonly ChromeNativeHostSetupService _setupService;
    private string _extensionId = string.Empty;
    private string _nativeHostExePath = string.Empty;
    private string _manifestPath = string.Empty;
    private string _registryStatus = string.Empty;
    private string _latestCaptureStatus = string.Empty;
    private string _diagnosticsText = string.Empty;
    private bool _latestCaptureFileExists;

    public ChromeNativeHostSetupDialogViewModel(ChromeNativeHostSetupService setupService)
    {
        _setupService = setupService;
        CheckStatusCommand = new AsyncRelayCommand(CheckStatusAsync);
        RegisterNativeHostCommand = new AsyncRelayCommand(RegisterNativeHostAsync);
        OpenChromeExtensionsCommand = new AsyncRelayCommand(OpenChromeExtensionsAsync);
        OpenExtensionFolderCommand = new AsyncRelayCommand(OpenExtensionFolderAsync);
        OpenLatestCaptureFileCommand = new AsyncRelayCommand(
            OpenLatestCaptureFileAsync,
            () => LatestCaptureFileExists);
        ApplyStatus(_setupService.CheckStatus(CreateRequest()));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public AsyncRelayCommand CheckStatusCommand { get; }

    public AsyncRelayCommand RegisterNativeHostCommand { get; }

    public AsyncRelayCommand OpenChromeExtensionsCommand { get; }

    public AsyncRelayCommand OpenExtensionFolderCommand { get; }

    public AsyncRelayCommand OpenLatestCaptureFileCommand { get; }

    public string ExtensionId
    {
        get => _extensionId;
        set
        {
            if (_extensionId == value)
            {
                return;
            }

            _extensionId = value.Trim();
            OnPropertyChanged();
        }
    }

    public string NativeHostExePath
    {
        get => _nativeHostExePath;
        private set => SetField(ref _nativeHostExePath, value);
    }

    public string ManifestPath
    {
        get => _manifestPath;
        private set => SetField(ref _manifestPath, value);
    }

    public string RegistryStatus
    {
        get => _registryStatus;
        private set => SetField(ref _registryStatus, value);
    }

    public string LatestCaptureStatus
    {
        get => _latestCaptureStatus;
        private set => SetField(ref _latestCaptureStatus, value);
    }

    public string DiagnosticsText
    {
        get => _diagnosticsText;
        private set => SetField(ref _diagnosticsText, value);
    }

    public bool LatestCaptureFileExists
    {
        get => _latestCaptureFileExists;
        private set
        {
            if (SetField(ref _latestCaptureFileExists, value))
            {
                OpenLatestCaptureFileCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private Task CheckStatusAsync()
    {
        ApplyStatus(_setupService.CheckStatus(CreateRequest()));
        return Task.CompletedTask;
    }

    private Task RegisterNativeHostAsync()
    {
        ApplyStatus(_setupService.Register(CreateRequest()));
        return Task.CompletedTask;
    }

    private async Task OpenChromeExtensionsAsync()
    {
        await OpenShellTargetAsync("chrome://extensions");
    }

    private async Task OpenExtensionFolderAsync()
    {
        string path = _setupService.ExtensionFolderPath;
        if (!Directory.Exists(path))
        {
            DiagnosticsText = $"Extension folder was not found: {path}";
            return;
        }

        await OpenShellTargetAsync(path);
    }

    private async Task OpenLatestCaptureFileAsync()
    {
        ChromeNativeHostSetupStatus status = _setupService.CheckStatus(CreateRequest());
        if (!status.LatestCaptureFileExists)
        {
            ApplyStatus(status);
            return;
        }

        await OpenShellTargetAsync(status.LatestCapturePath);
    }

    private void ApplyStatus(ChromeNativeHostSetupStatus status)
    {
        NativeHostExePath = status.NativeHostExePath;
        ManifestPath = status.ManifestPath;
        RegistryStatus = status.RegistryKeyExists
            ? $"Registered: {status.RegisteredManifestPath}"
            : "Not registered under HKCU.";
        LatestCaptureStatus = status.LatestCaptureFileExists
            ? $"Found: {status.LatestCapturePath}"
            : $"Not found: {status.LatestCapturePath}";
        LatestCaptureFileExists = status.LatestCaptureFileExists;

        List<string> lines = [];
        if (!string.IsNullOrWhiteSpace(status.AllowedOrigin))
        {
            lines.Add($"Allowed origin: {status.AllowedOrigin}");
        }

        lines.Add($"Native host exe exists: {status.NativeHostExeExists}");
        lines.Add($"Manifest exists: {status.ManifestFileExists}");
        lines.Add($"Manifest JSON valid: {status.ManifestJsonValid}");
        lines.Add($"Registry points to expected manifest: {status.RegistryValuePointsToExpectedManifest}");
        lines.AddRange(status.Errors.Select(error => $"Error: {error}"));
        lines.AddRange(status.Warnings.Select(warning => $"Note: {warning}"));
        DiagnosticsText = string.Join(Environment.NewLine, lines);
    }

    private ChromeNativeHostSetupRequest CreateRequest()
    {
        return new ChromeNativeHostSetupRequest { ExtensionId = ExtensionId };
    }

    private async Task OpenShellTargetAsync(string target)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            DiagnosticsText = $"Open failed: {ex.Message}";
        }

        await Task.CompletedTask;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
