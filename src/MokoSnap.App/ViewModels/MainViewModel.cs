using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MokoSnap.App.Services;
using MokoSnap.Core.Capture;
using MokoSnap.Core.ChromeCapture;
using MokoSnap.Core.Hotkeys;
using MokoSnap.Core.Models;
using MokoSnap.Core.Running;
using MokoSnap.Core.Storage;

namespace MokoSnap.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly IJsonStorage<AppSettings> _settingsStorage;
    private readonly IConfirmationService _confirmationService;
    private readonly ICapturedAppSelectionService _capturedAppSelectionService;
    private readonly IChromeTabCaptureSelectionService _chromeTabCaptureSelectionService;
    private readonly PresetRunnerService _presetRunnerService;
    private readonly IHotkeyService _hotkeyService;
    private readonly ICommandPaletteService _commandPaletteService;
    private PresetEditorViewModel? _selectedPreset;
    private string _statusMessage = string.Empty;
    private string _runResultMessage = string.Empty;
    private bool _isRunning;
    private bool _launchOnStartup;
    private bool _startMinimizedToTray;
    private bool _minimizeToTray = true;

    public MainViewModel(
        IJsonStorage<AppSettings> settingsStorage,
        IConfirmationService confirmationService,
        ICapturedAppSelectionService capturedAppSelectionService,
        IChromeTabCaptureSelectionService chromeTabCaptureSelectionService,
        PresetRunnerService presetRunnerService,
        IHotkeyService hotkeyService,
        ICommandPaletteService commandPaletteService)
    {
        _settingsStorage = settingsStorage;
        _confirmationService = confirmationService;
        _capturedAppSelectionService = capturedAppSelectionService;
        _chromeTabCaptureSelectionService = chromeTabCaptureSelectionService;
        _presetRunnerService = presetRunnerService;
        _hotkeyService = hotkeyService;
        _commandPaletteService = commandPaletteService;
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new AsyncRelayCommand(AddAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => SelectedPreset is not null);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedPreset is not null);
        CaptureCurrentAppsCommand = new AsyncRelayCommand(CaptureCurrentAppsAsync, () => SelectedPreset is not null);
        ImportLatestChromeTabsCommand = new AsyncRelayCommand(ImportLatestChromeTabsAsync, () => SelectedPreset is not null);
        RunCommand = new AsyncRelayCommand(RunAsync, () => SelectedPreset is not null && !IsRunning);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<PresetEditorViewModel> Presets { get; } = [];

    public Array ClosePolicies { get; } = Enum.GetValues(typeof(ClosePolicy));

    public Array CloseConfirmationPolicies { get; } = Enum.GetValues(typeof(CloseConfirmationPolicy));

    public AsyncRelayCommand LoadCommand { get; }

    public AsyncRelayCommand AddCommand { get; }

    public AsyncRelayCommand SaveCommand { get; }

    public AsyncRelayCommand DeleteCommand { get; }

    public AsyncRelayCommand CaptureCurrentAppsCommand { get; }

    public AsyncRelayCommand ImportLatestChromeTabsCommand { get; }

    public AsyncRelayCommand RunCommand { get; }

    public PresetEditorViewModel? SelectedPreset
    {
        get => _selectedPreset;
        set
        {
            if (_selectedPreset == value)
            {
                return;
            }

            _selectedPreset = value;
            OnPropertyChanged();
            SaveCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
            CaptureCurrentAppsCommand.RaiseCanExecuteChanged();
            ImportLatestChromeTabsCommand.RaiseCanExecuteChanged();
            RunCommand.RaiseCanExecuteChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public string RunResultMessage
    {
        get => _runResultMessage;
        private set
        {
            _runResultMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (_isRunning == value)
            {
                return;
            }

            _isRunning = value;
            OnPropertyChanged();
            RunCommand.RaiseCanExecuteChanged();
        }
    }

    public bool LaunchOnStartup
    {
        get => _launchOnStartup;
        private set
        {
            _launchOnStartup = value;
            OnPropertyChanged();
        }
    }

    public bool StartMinimizedToTray
    {
        get => _startMinimizedToTray;
        private set
        {
            _startMinimizedToTray = value;
            OnPropertyChanged();
        }
    }

    public bool MinimizeToTray
    {
        get => _minimizeToTray;
        private set
        {
            _minimizeToTray = value;
            OnPropertyChanged();
        }
    }

    public async Task LoadAsync()
    {
        AppSettings settings = await _settingsStorage.LoadAsync();
        LaunchOnStartup = settings.LaunchOnStartup;
        StartMinimizedToTray = settings.StartMinimizedToTray;
        MinimizeToTray = settings.MinimizeToTray;
        Presets.Clear();

        foreach (Preset preset in settings.Presets)
        {
            Presets.Add(new PresetEditorViewModel(preset));
        }

        SelectedPreset = Presets.FirstOrDefault();
        List<string> hotkeyMessages = RefreshHotkeyRegistrations(settings.Presets);
        StatusMessage = BuildStatusWithHotkeySummary(
            Presets.Count == 0 ? "No presets yet." : $"Loaded {Presets.Count} preset(s).",
            settings.Presets,
            hotkeyMessages);
    }

    private async Task AddAsync()
    {
        PresetEditorViewModel preset = new(new Preset
        {
            Name = "New Preset"
        });

        Presets.Add(preset);
        SelectedPreset = preset;
        List<Preset> presets = CreatePresets();
        await SaveSettingsAsync(presets);
        List<string> hotkeyMessages = RefreshHotkeyRegistrations(presets);
        StatusMessage = BuildStatusWithHotkeySummary("Preset added.", presets, hotkeyMessages);
    }

    private async Task SaveAsync()
    {
        if (SelectedPreset is null)
        {
            return;
        }

        List<Preset> presets = CreatePresets();
        HotkeyValidationResult validation = HotkeyValidator.ValidatePresets(presets);
        if (!validation.IsValid)
        {
            StatusMessage = $"Preset not saved. {validation.Errors[0].Message}";
            return;
        }

        await SaveSettingsAsync(presets);
        List<string> hotkeyMessages = RefreshHotkeyRegistrations(presets);
        StatusMessage = BuildStatusWithHotkeySummary("Preset saved.", presets, hotkeyMessages);
    }

    private async Task DeleteAsync()
    {
        if (SelectedPreset is null)
        {
            return;
        }

        string name = string.IsNullOrWhiteSpace(SelectedPreset.Name) ? "this preset" : SelectedPreset.Name;
        if (!_confirmationService.Confirm($"Delete {name}?", "Delete Preset"))
        {
            return;
        }

        int index = Presets.IndexOf(SelectedPreset);
        Presets.Remove(SelectedPreset);
        SelectedPreset = Presets.Count == 0 ? null : Presets[Math.Min(index, Presets.Count - 1)];

        List<Preset> presets = CreatePresets();
        await SaveSettingsAsync(presets);
        List<string> hotkeyMessages = RefreshHotkeyRegistrations(presets);
        StatusMessage = BuildStatusWithHotkeySummary("Preset deleted.", presets, hotkeyMessages);
    }

    private async Task CaptureCurrentAppsAsync()
    {
        if (SelectedPreset is null)
        {
            return;
        }

        IReadOnlyList<CapturedWindowApp> selectedApps = await _capturedAppSelectionService.SelectCapturedAppsAsync();
        if (selectedApps.Count == 0)
        {
            StatusMessage = "No captured apps added.";
            return;
        }

        SelectedPreset.AppendCapturedApps(selectedApps);
        List<Preset> presets = CreatePresets();
        await SaveSettingsAsync(presets);
        List<string> hotkeyMessages = RefreshHotkeyRegistrations(presets);
        StatusMessage = BuildStatusWithHotkeySummary(
            $"Captured {selectedApps.Count} app target(s).",
            presets,
            hotkeyMessages);
    }

    private async Task ImportLatestChromeTabsAsync()
    {
        if (SelectedPreset is null)
        {
            return;
        }

        ChromeTabCaptureSelectionResult selectionResult =
            await _chromeTabCaptureSelectionService.SelectLatestCaptureAsync();
        if (!selectionResult.Succeeded)
        {
            StatusMessage = selectionResult.Message;
            return;
        }

        TargetConfig chromeTarget = ChromeTabCaptureImporter.CreateChromeTarget(selectionResult.SelectedTabs);
        SelectedPreset.AppendChromeTarget(chromeTarget);
        List<Preset> presets = CreatePresets();
        await SaveSettingsAsync(presets);
        List<string> hotkeyMessages = RefreshHotkeyRegistrations(presets);
        StatusMessage = BuildStatusWithHotkeySummary(
            $"Imported {chromeTarget.Urls.Count} Chrome tab(s).",
            presets,
            hotkeyMessages);
    }

    private async Task RunAsync()
    {
        if (SelectedPreset is null)
        {
            return;
        }

        await RunPresetAsync(SelectedPreset);
    }

    public async Task OpenCommandPaletteAsync()
    {
        PresetEditorViewModel? selected = _commandPaletteService.SelectPreset(Presets.ToList());
        if (selected is null)
        {
            return;
        }

        SelectedPreset = selected;
        await RunPresetAsync(selected);
    }

    public async Task RunPresetByIdAsync(string presetId)
    {
        PresetEditorViewModel? preset = Presets.FirstOrDefault(item => item.Id == presetId);
        if (preset is null)
        {
            return;
        }

        SelectedPreset = preset;
        await RunPresetAsync(preset);
    }

    private async void OnHotkeyPressed(object? sender, HotkeyPressedEventArgs e)
    {
        if (e.IsCommandPalette)
        {
            await OpenCommandPaletteAsync();
            return;
        }

        if (e.PresetId is not null)
        {
            await RunPresetByIdAsync(e.PresetId);
        }
    }

    private async Task RunPresetAsync(PresetEditorViewModel presetEditor)
    {
        if (IsRunning)
        {
            StatusMessage = "Preset already running.";
            return;
        }

        Preset preset = presetEditor.ToPreset();
        try
        {
            IsRunning = true;
            StatusMessage = "Running preset...";
            RunResultMessage = string.Empty;

            PresetRunResult result = await _presetRunnerService.RunAsync(preset);
            RunResultMessage = FormatRunResult(result);
            StatusMessage = result.CloseWindowsResult?.Canceled == true
                ? "Preset run canceled."
                : result.Succeeded ? "Preset run completed." : "Preset run completed with failures.";
        }
        catch (Exception ex)
        {
            RunResultMessage = $"Preset run failed: {ex.Message}";
            StatusMessage = "Preset run failed.";
        }
        finally
        {
            IsRunning = false;
        }
    }

    private static string FormatRunResult(PresetRunResult result)
    {
        int successCount = result.TargetResults.Count(target => target.Succeeded);
        int failureCount = result.TargetResults.Count - successCount;
        List<string> lines =
        [
            $"Successful targets: {successCount}",
            $"Failed targets: {failureCount}"
        ];

        if (result.CloseWindowsResult is not null)
        {
            CloseWindowsResult closeResult = result.CloseWindowsResult;
            lines.Add(
                $"Closed windows: {closeResult.ClosedWindowCount}; failed: {closeResult.FailedWindows.Count}; skipped: {closeResult.SkippedWindows.Count}");
            if (closeResult.Canceled)
            {
                lines.Add("Window closing canceled. Targets were not launched.");
            }
            else if (!string.IsNullOrWhiteSpace(closeResult.Message))
            {
                lines.Add(closeResult.Message);
            }

            foreach (CloseWindowFailure failure in closeResult.FailedWindows)
            {
                string title = string.IsNullOrWhiteSpace(failure.Window.WindowTitle)
                    ? failure.Window.ProcessName
                    : failure.Window.WindowTitle;
                lines.Add($"Close failed: {title} - {failure.Message}");
            }
        }

        foreach (TargetRunResult targetResult in result.TargetResults)
        {
            string targetName = string.IsNullOrWhiteSpace(targetResult.Target.DisplayName)
                ? targetResult.Target.Type.ToString()
                : targetResult.Target.DisplayName;
            string status = targetResult.Succeeded ? "OK" : "Failed";
            string message = string.IsNullOrWhiteSpace(targetResult.Message) ? string.Empty : $" - {targetResult.Message}";
            lines.Add($"{status}: {targetName}{message}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private Task SaveSettingsAsync()
    {
        return SaveSettingsAsync(CreatePresets());
    }

    private Task SaveSettingsAsync(IReadOnlyList<Preset> presets)
    {
        AppSettings settings = new()
        {
            LaunchOnStartup = LaunchOnStartup,
            StartMinimizedToTray = StartMinimizedToTray,
            MinimizeToTray = MinimizeToTray,
            Presets = presets.ToList()
        };

        return _settingsStorage.SaveAsync(settings);
    }

    private List<Preset> CreatePresets()
    {
        return Presets.Select(preset => preset.ToPreset()).ToList();
    }

    private List<string> RefreshHotkeyRegistrations(IReadOnlyList<Preset> presets)
    {
        List<string> messages = [];
        _hotkeyService.UnregisterAll();
        HotkeyRegistrationResult commandPaletteResult = _hotkeyService.RegisterCommandPaletteHotkey(new HotkeyGesture
        {
            Key = "Space",
            Ctrl = true,
            Alt = true
        });
        AddRegistrationFailure(messages, commandPaletteResult, "Command palette");

        HotkeyValidationResult validation = HotkeyValidator.ValidatePresets(presets);
        foreach (Preset preset in presets)
        {
            if (preset.Hotkey is null || string.IsNullOrWhiteSpace(preset.Hotkey.Key))
            {
                continue;
            }

            if (validation.Errors.Any(error => error.PresetId == preset.Id))
            {
                continue;
            }

            HotkeyRegistrationResult result = _hotkeyService.RegisterPresetHotkey(preset.Id, preset.Name, preset.Hotkey);
            AddRegistrationFailure(messages, result, string.IsNullOrWhiteSpace(preset.Name) ? "Unnamed preset" : preset.Name);
        }

        return messages;
    }

    private static void AddRegistrationFailure(
        List<string> messages,
        HotkeyRegistrationResult result,
        string displayName)
    {
        if (result.Success)
        {
            return;
        }

        string conflict = string.IsNullOrWhiteSpace(result.ConflictingPresetName)
            ? string.Empty
            : $" Conflicts with {result.ConflictingPresetName}.";
        messages.Add($"{displayName} hotkey not registered: {result.ErrorMessage}{conflict}");
    }

    private static string BuildStatusWithHotkeySummary(
        string status,
        IReadOnlyList<Preset> presets,
        IReadOnlyList<string> registrationMessages)
    {
        if (registrationMessages.Count > 0)
        {
            return $"{status} {registrationMessages[0]}";
        }

        HotkeyValidationResult validation = HotkeyValidator.ValidatePresets(presets);
        if (validation.Errors.Count > 0)
        {
            return $"{status} Hotkey issue: {validation.Errors[0].Message}";
        }

        if (validation.Warnings.Count > 0)
        {
            return $"{status} Hotkey warning: {validation.Warnings[0].Message}";
        }

        return status;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
