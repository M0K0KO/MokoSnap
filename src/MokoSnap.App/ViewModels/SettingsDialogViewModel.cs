using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MokoSnap.Core.Hotkeys;
using MokoSnap.Core.Models;

namespace MokoSnap.App.ViewModels;

public sealed class SettingsDialogViewModel : INotifyPropertyChanged
{
    private readonly IReadOnlyList<Preset> _presets;
    private readonly string? _registeredStartupCommand;
    private readonly string _expectedStartupCommand;
    private readonly string _startupStatusError;
    private readonly string _hotkeyStatusText;
    private readonly Action _openChromeCaptureSetup;
    private string _quickSwitcherHotkeyText;
    private bool _launchOnStartup;
    private bool _startMinimizedToTray;
    private bool _minimizeToTray;
    private string _hotkeyValidationText = string.Empty;
    private string _startupStatusText = string.Empty;

    public SettingsDialogViewModel(SettingsDialogRequest request, Action openChromeCaptureSetup)
    {
        _presets = request.Presets;
        _registeredStartupCommand = request.RegisteredStartupCommand;
        _expectedStartupCommand = request.ExpectedStartupCommand;
        _startupStatusError = request.StartupStatusError;
        _hotkeyStatusText = request.HotkeyStatusText;
        _openChromeCaptureSetup = openChromeCaptureSetup;
        _quickSwitcherHotkeyText = HotkeyGestureFormatter.Format(request.QuickSwitcherHotkey);
        _launchOnStartup = request.LaunchOnStartup;
        _startMinimizedToTray = request.StartMinimizedToTray;
        _minimizeToTray = request.MinimizeToTray;
        ChromeCaptureStatusText = request.ChromeCaptureStatusText;
        OpenChromeCaptureSetupCommand = new RelayCommand(_openChromeCaptureSetup);
        OpenChromeExtensionsCommand = new RelayCommand(OpenChromeExtensions);
        ResetQuickSwitcherHotkeyCommand = new RelayCommand(ResetQuickSwitcherHotkey);
        RefreshValidation();
        RefreshStartupStatus();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public RelayCommand OpenChromeCaptureSetupCommand { get; }

    public RelayCommand OpenChromeExtensionsCommand { get; }

    public RelayCommand ResetQuickSwitcherHotkeyCommand { get; }

    public string QuickSwitcherHotkeyText
    {
        get => _quickSwitcherHotkeyText;
        set
        {
            if (SetField(ref _quickSwitcherHotkeyText, value))
            {
                RefreshValidation();
            }
        }
    }

    public bool LaunchOnStartup
    {
        get => _launchOnStartup;
        set
        {
            if (SetField(ref _launchOnStartup, value))
            {
                RefreshStartupStatus();
            }
        }
    }

    public bool StartMinimizedToTray
    {
        get => _startMinimizedToTray;
        set
        {
            if (SetField(ref _startMinimizedToTray, value))
            {
                RefreshStartupStatus();
            }
        }
    }

    public bool MinimizeToTray
    {
        get => _minimizeToTray;
        set => SetField(ref _minimizeToTray, value);
    }

    public string HotkeyValidationText
    {
        get => _hotkeyValidationText;
        private set => SetField(ref _hotkeyValidationText, value);
    }

    public string StartupStatusText
    {
        get => _startupStatusText;
        private set => SetField(ref _startupStatusText, value);
    }

    public string ChromeCaptureStatusText { get; }

    public bool TryCreateResult(out SettingsDialogResult? result)
    {
        HotkeyGesture quickSwitcherHotkey = QuickSwitcherHotkeyDefaults.Resolve(
            HotkeyGestureFormatter.Parse(QuickSwitcherHotkeyText));
        HotkeyValidationResult validation = HotkeyValidator.ValidateQuickSwitcherHotkey(
            quickSwitcherHotkey,
            _presets);
        if (!validation.IsValid)
        {
            RefreshValidation();
            result = null;
            return false;
        }

        result = new SettingsDialogResult
        {
            QuickSwitcherHotkey = quickSwitcherHotkey,
            LaunchOnStartup = LaunchOnStartup,
            StartMinimizedToTray = StartMinimizedToTray,
            MinimizeToTray = MinimizeToTray
        };
        return true;
    }

    private void RefreshValidation()
    {
        HotkeyGesture quickSwitcherHotkey = QuickSwitcherHotkeyDefaults.Resolve(
            HotkeyGestureFormatter.Parse(QuickSwitcherHotkeyText));
        List<string> lines = [];
        lines.AddRange(HotkeyValidator
            .ValidateQuickSwitcherHotkey(quickSwitcherHotkey, _presets)
            .Messages
            .Select(message => message.Message));
        lines.AddRange(HotkeyValidator
            .ValidatePresets(_presets)
            .Errors
            .Select(error => error.Message));
        if (!string.IsNullOrWhiteSpace(_hotkeyStatusText))
        {
            lines.Add(_hotkeyStatusText);
        }

        HotkeyValidationText = lines.Count == 0
            ? $"Quick Switcher hotkey: {HotkeyGestureFormatter.Format(quickSwitcherHotkey)}"
            : string.Join(Environment.NewLine, lines);
    }

    private void RefreshStartupStatus()
    {
        if (!string.IsNullOrWhiteSpace(_startupStatusError))
        {
            StartupStatusText = $"Error: {_startupStatusError}";
            return;
        }

        string current = string.IsNullOrWhiteSpace(_registeredStartupCommand)
            ? "Disabled"
            : string.Equals(_registeredStartupCommand, _expectedStartupCommand, StringComparison.OrdinalIgnoreCase)
                ? "Enabled"
                : $"Registered path mismatch: {_registeredStartupCommand}";
        StartupStatusText = $"{current}. Startup changes apply on Save.";
    }

    private void ResetQuickSwitcherHotkey()
    {
        QuickSwitcherHotkeyText = HotkeyGestureFormatter.Format(QuickSwitcherHotkeyDefaults.CreateDefault());
    }

    private static void OpenChromeExtensions()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "chrome://extensions",
                UseShellExecute = true
            });
        }
        catch
        {
        }
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
