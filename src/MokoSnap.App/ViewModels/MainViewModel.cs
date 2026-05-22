using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MokoSnap.App.Services;
using MokoSnap.Core.Capture;
using MokoSnap.Core.ChromeCapture;
using MokoSnap.Core.Hotkeys;
using MokoSnap.Core.Models;
using MokoSnap.Core.Onboarding;
using MokoSnap.Core.Running;
using MokoSnap.Core.Storage;
using MokoSnap.Core.Startup;

namespace MokoSnap.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly IJsonStorage<AppSettings> _settingsStorage;
    private readonly IConfirmationService _confirmationService;
    private readonly ICapturedAppSelectionService _capturedAppSelectionService;
    private readonly IChromeTabCaptureSelectionService _chromeTabCaptureSelectionService;
    private readonly IChromeNativeHostSetupDialogService _chromeNativeHostSetupDialogService;
    private readonly PresetRunnerService _presetRunnerService;
    private readonly IHotkeyService _hotkeyService;
    private readonly ICommandPaletteService _commandPaletteService;
    private readonly IStartupRegistrationService _startupRegistrationService;
    private readonly ISettingsDialogService _settingsDialogService;
    private readonly IOnboardingDialogService _onboardingDialogService;
    private readonly IChromeCaptureDiagnosticsService _chromeCaptureDiagnosticsService;
    private PresetEditorViewModel? _selectedPreset;
    private string _statusMessage = string.Empty;
    private string _runResultMessage = string.Empty;
    private string _selectedShellSection = ShellSectionPresets;
    private string _quickSwitcherHotkeyStatus = "Not registered yet.";
    private string _singleInstanceStatus = "Active primary instance.";
    private string _chromeCaptureStatus = string.Empty;
    private string _diagnosticsText = string.Empty;
    private bool _isRunning;
    private bool _launchOnStartup;
    private bool _startMinimizedToTray;
    private bool _minimizeToTray = true;
    private bool _hasSeenFirstRunOnboarding;
    private bool _settingsCanBeSaved = true;
    private HotkeyGesture _quickSwitcherHotkey = QuickSwitcherHotkeyDefaults.CreateDefault();
    private List<string> _lastHotkeyMessages = [];

    public MainViewModel(
        IJsonStorage<AppSettings> settingsStorage,
        IConfirmationService confirmationService,
        ICapturedAppSelectionService capturedAppSelectionService,
        IChromeTabCaptureSelectionService chromeTabCaptureSelectionService,
        IChromeNativeHostSetupDialogService chromeNativeHostSetupDialogService,
        PresetRunnerService presetRunnerService,
        IHotkeyService hotkeyService,
        ICommandPaletteService commandPaletteService,
        IStartupRegistrationService startupRegistrationService,
        ISettingsDialogService settingsDialogService,
        IOnboardingDialogService onboardingDialogService,
        IChromeCaptureDiagnosticsService chromeCaptureDiagnosticsService)
    {
        _settingsStorage = settingsStorage;
        _confirmationService = confirmationService;
        _capturedAppSelectionService = capturedAppSelectionService;
        _chromeTabCaptureSelectionService = chromeTabCaptureSelectionService;
        _chromeNativeHostSetupDialogService = chromeNativeHostSetupDialogService;
        _presetRunnerService = presetRunnerService;
        _hotkeyService = hotkeyService;
        _commandPaletteService = commandPaletteService;
        _startupRegistrationService = startupRegistrationService;
        _settingsDialogService = settingsDialogService;
        _onboardingDialogService = onboardingDialogService;
        _chromeCaptureDiagnosticsService = chromeCaptureDiagnosticsService;
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new AsyncRelayCommand(AddAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => SelectedPreset is not null);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedPreset is not null);
        CaptureCurrentAppsCommand = new AsyncRelayCommand(CaptureCurrentAppsAsync, () => SelectedPreset is not null);
        ImportLatestChromeTabsCommand = new AsyncRelayCommand(ImportLatestChromeTabsAsync, () => SelectedPreset is not null);
        ChromeCaptureSetupCommand = new RelayCommand(OpenChromeCaptureSetup);
        SaveStartupSettingsCommand = new AsyncRelayCommand(SaveStartupSettingsAsync);
        OpenSettingsCommand = new AsyncRelayCommand(OpenSettingsAsync);
        OpenGettingStartedCommand = new AsyncRelayCommand(OpenGettingStartedAsync);
        ShowPresetsSectionCommand = new RelayCommand(() => SelectedShellSection = ShellSectionPresets);
        ShowSettingsSectionCommand = new RelayCommand(() => SelectedShellSection = ShellSectionSettings);
        ShowChromeCaptureSectionCommand = new RelayCommand(() => SelectedShellSection = ShellSectionChromeCapture);
        ShowHelpSectionCommand = new RelayCommand(() => SelectedShellSection = ShellSectionHelp);
        RunCommand = new AsyncRelayCommand(RunAsync, () => SelectedPreset is not null && !IsRunning);
        RefreshDiagnostics();
    }

    private const string ShellSectionPresets = "Presets";
    private const string ShellSectionSettings = "Settings";
    private const string ShellSectionChromeCapture = "Chrome Capture";
    private const string ShellSectionHelp = "Help";

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

    public RelayCommand ChromeCaptureSetupCommand { get; }

    public AsyncRelayCommand SaveStartupSettingsCommand { get; }

    public AsyncRelayCommand OpenSettingsCommand { get; }

    public AsyncRelayCommand OpenGettingStartedCommand { get; }

    public RelayCommand ShowPresetsSectionCommand { get; }

    public RelayCommand ShowSettingsSectionCommand { get; }

    public RelayCommand ShowChromeCaptureSectionCommand { get; }

    public RelayCommand ShowHelpSectionCommand { get; }

    public AsyncRelayCommand RunCommand { get; }

    public string SelectedShellSection
    {
        get => _selectedShellSection;
        set
        {
            if (!SetField(ref _selectedShellSection, value))
            {
                return;
            }

            OnPropertyChanged(nameof(IsPresetsSectionSelected));
            OnPropertyChanged(nameof(IsSettingsSectionSelected));
            OnPropertyChanged(nameof(IsChromeCaptureSectionSelected));
            OnPropertyChanged(nameof(IsHelpSectionSelected));
        }
    }

    public bool IsPresetsSectionSelected => SelectedShellSection == ShellSectionPresets;

    public bool IsSettingsSectionSelected => SelectedShellSection == ShellSectionSettings;

    public bool IsChromeCaptureSectionSelected => SelectedShellSection == ShellSectionChromeCapture;

    public bool IsHelpSectionSelected => SelectedShellSection == ShellSectionHelp;

    public string QuickSwitcherHotkeySummary => HotkeyGestureFormatter.Format(_quickSwitcherHotkey);

    public string QuickSwitcherHotkeyStatus
    {
        get => _quickSwitcherHotkeyStatus;
        private set => SetField(ref _quickSwitcherHotkeyStatus, value);
    }

    public string SingleInstanceStatus
    {
        get => _singleInstanceStatus;
        private set => SetField(ref _singleInstanceStatus, value);
    }

    public string TrayStatus => "Running.";

    public string ChromeCaptureStatus
    {
        get => _chromeCaptureStatus;
        private set => SetField(ref _chromeCaptureStatus, value);
    }

    public string DiagnosticsText
    {
        get => _diagnosticsText;
        private set => SetField(ref _diagnosticsText, value);
    }

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
        set
        {
            SetField(ref _launchOnStartup, value);
        }
    }

    public bool StartMinimizedToTray
    {
        get => _startMinimizedToTray;
        set
        {
            SetField(ref _startMinimizedToTray, value);
        }
    }

    public bool MinimizeToTray
    {
        get => _minimizeToTray;
        set
        {
            SetField(ref _minimizeToTray, value);
        }
    }

    public async Task LoadAsync()
    {
        AppSettings settings;
        string loadWarning = string.Empty;
        try
        {
            settings = await _settingsStorage.LoadAsync();
            _settingsCanBeSaved = true;
        }
        catch (Exception ex) when (ex is System.IO.IOException or System.Text.Json.JsonException or UnauthorizedAccessException)
        {
            settings = AppSettings.CreateDefault();
            _settingsCanBeSaved = false;
            loadWarning =
                $"Settings could not be loaded. Using in-memory defaults and blocking saves to avoid overwriting existing data. {ex.Message}";
        }

        bool registeredForStartup = false;
        try
        {
            registeredForStartup = _startupRegistrationService.IsRegistered();
        }
        catch
        {
            registeredForStartup = false;
        }

        LaunchOnStartup = settings.LaunchOnStartup || registeredForStartup;
        StartMinimizedToTray = settings.StartMinimizedToTray;
        MinimizeToTray = settings.MinimizeToTray;
        _hasSeenFirstRunOnboarding = settings.HasSeenFirstRunOnboarding;
        bool quickSwitcherHotkeyMigrated = QuickSwitcherHotkeyDefaults.ShouldUseDefault(settings.QuickSwitcherHotkey);
        _quickSwitcherHotkey = QuickSwitcherHotkeyDefaults.Resolve(settings.QuickSwitcherHotkey);
        string migrationWarning = string.Empty;
        if (LaunchOnStartup)
        {
            try
            {
                _startupRegistrationService.SetLaunchOnStartup(true, StartMinimizedToTray);
            }
            catch
            {
                LaunchOnStartup = false;
            }
        }

        if (quickSwitcherHotkeyMigrated)
        {
            settings.QuickSwitcherHotkey = _quickSwitcherHotkey;
            if (!await TrySaveSettingsAsync("Quick Switcher hotkey migration was not saved.", settings.Presets))
            {
                migrationWarning = StatusMessage;
            }
        }

        Presets.Clear();

        foreach (Preset preset in settings.Presets)
        {
            Presets.Add(new PresetEditorViewModel(preset));
        }

        SelectedPreset = Presets.FirstOrDefault();
        List<string> hotkeyMessages = RefreshHotkeyRegistrations(settings.Presets);
        string loadedStatus = BuildStatusWithHotkeySummary(
            Presets.Count == 0 ? "No presets yet." : $"Loaded {Presets.Count} preset(s).",
            settings.Presets,
            hotkeyMessages);
        StatusMessage = !string.IsNullOrWhiteSpace(loadWarning)
            ? loadWarning
            : string.IsNullOrWhiteSpace(migrationWarning) ? loadedStatus : migrationWarning;
        RefreshDiagnostics();
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
        if (!await TrySaveSettingsAsync("Preset was not saved.", presets))
        {
            return;
        }

        List<string> hotkeyMessages = RefreshHotkeyRegistrations(presets);
        StatusMessage = BuildStatusWithHotkeySummary("Preset added.", presets, hotkeyMessages);
        RefreshDiagnostics();
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

        if (!await TrySaveSettingsAsync("Preset was not saved.", presets))
        {
            return;
        }

        List<string> hotkeyMessages = RefreshHotkeyRegistrations(presets);
        StatusMessage = BuildStatusWithHotkeySummary("Preset saved.", presets, hotkeyMessages);
        RefreshDiagnostics();
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
        if (!await TrySaveSettingsAsync("Preset deletion was not saved.", presets))
        {
            return;
        }

        List<string> hotkeyMessages = RefreshHotkeyRegistrations(presets);
        StatusMessage = BuildStatusWithHotkeySummary("Preset deleted.", presets, hotkeyMessages);
        RefreshDiagnostics();
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
        if (!await TrySaveSettingsAsync("Captured apps were not saved.", presets))
        {
            return;
        }

        List<string> hotkeyMessages = RefreshHotkeyRegistrations(presets);
        StatusMessage = BuildStatusWithHotkeySummary(
            $"Captured {selectedApps.Count} app target(s).",
            presets,
            hotkeyMessages);
        RefreshDiagnostics();
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
        if (!await TrySaveSettingsAsync("Chrome tabs were not saved.", presets))
        {
            return;
        }

        List<string> hotkeyMessages = RefreshHotkeyRegistrations(presets);
        StatusMessage = BuildStatusWithHotkeySummary(
            $"Imported {chromeTarget.Urls.Count} Chrome tab(s).",
            presets,
            hotkeyMessages);
        RefreshDiagnostics();
    }

    private void OpenChromeCaptureSetup()
    {
        _chromeNativeHostSetupDialogService.ShowSetupDialog();
        RefreshDiagnostics();
    }

    private async Task SaveStartupSettingsAsync()
    {
        try
        {
            _startupRegistrationService.SetLaunchOnStartup(LaunchOnStartup, StartMinimizedToTray);
            if (!await TrySaveSettingsAsync("Startup settings were not saved."))
            {
                return;
            }

            StatusMessage = LaunchOnStartup
                ? "Startup registration saved for current user."
                : "Startup registration disabled for current user.";
            RefreshDiagnostics();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Startup registration failed: {ex.Message}";
            RefreshDiagnostics();
        }
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
        CommandPaletteSelection selection = _commandPaletteService.SelectPreset(Presets.ToList());
        if (selection.OpensSettings)
        {
            await OpenSettingsAsync();
            return;
        }

        if (selection.Preset is null)
        {
            return;
        }

        SelectedPreset = selection.Preset;
        await RunPresetAsync(selection.Preset);
    }

    public Task OpenSettingsAsync()
    {
        try
        {
            SettingsDialogResult? result = _settingsDialogService.ShowSettings(CreateSettingsDialogRequest());
            return result is null ? Task.CompletedTask : ApplySettingsAsync(result);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Settings failed to open: {ex.Message}";
            RefreshDiagnostics();
            return Task.CompletedTask;
        }
    }

    public Task OpenGettingStartedAsync()
    {
        return ShowOnboardingAsync();
    }

    public Task ShowFirstRunOnboardingIfNeededAsync(bool startsMinimizedToTray)
    {
        return FirstRunOnboardingState.ShouldShow(_hasSeenFirstRunOnboarding, startsMinimizedToTray)
            ? ShowOnboardingAsync()
            : Task.CompletedTask;
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

    public void NoteActivationRequest()
    {
        SingleInstanceStatus = "Active primary instance. Last second-launch activation request received.";
        StatusMessage = "Existing MokoSnap instance activated.";
        RefreshDiagnostics();
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
            RefreshDiagnostics();
        }
        catch (Exception ex)
        {
            RunResultMessage = $"Preset run failed: {ex.Message}";
            StatusMessage = "Preset run failed.";
            RefreshDiagnostics();
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
            HasSeenFirstRunOnboarding = _hasSeenFirstRunOnboarding,
            QuickSwitcherHotkey = _quickSwitcherHotkey,
            Presets = presets.ToList()
        };

        if (!_settingsCanBeSaved)
        {
            throw new InvalidOperationException(
                "Settings were not saved because the existing settings file could not be loaded. Fix or rename appsettings.json, then restart MokoSnap.");
        }

        return _settingsStorage.SaveAsync(settings);
    }

    private async Task<bool> TrySaveSettingsAsync(string failurePrefix, IReadOnlyList<Preset>? presets = null)
    {
        try
        {
            if (presets is null)
            {
                await SaveSettingsAsync();
            }
            else
            {
                await SaveSettingsAsync(presets);
            }

            return true;
        }
        catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            StatusMessage = $"{failurePrefix} {ex.Message}";
            RefreshDiagnostics();
            return false;
        }
    }

    private SettingsDialogRequest CreateSettingsDialogRequest()
    {
        string? registeredStartupCommand = null;
        string expectedStartupCommand = string.Empty;
        string startupStatusError = string.Empty;
        try
        {
            registeredStartupCommand = _startupRegistrationService.GetRegisteredCommand();
            expectedStartupCommand = _startupRegistrationService.GetExpectedCommand(StartMinimizedToTray);
        }
        catch (Exception ex)
        {
            startupStatusError = ex.Message;
        }

        return new SettingsDialogRequest
        {
            QuickSwitcherHotkey = CloneHotkey(_quickSwitcherHotkey),
            LaunchOnStartup = LaunchOnStartup,
            StartMinimizedToTray = StartMinimizedToTray,
            MinimizeToTray = MinimizeToTray,
            Presets = CreatePresets(),
            RegisteredStartupCommand = registeredStartupCommand,
            ExpectedStartupCommand = expectedStartupCommand,
            StartupStatusError = startupStatusError,
            HotkeyStatusText = string.Join(Environment.NewLine, _lastHotkeyMessages)
        };
    }

    private async Task ApplySettingsAsync(SettingsDialogResult result)
    {
        HotkeyGesture previousQuickSwitcherHotkey = _quickSwitcherHotkey;
        bool previousLaunchOnStartup = LaunchOnStartup;
        bool previousStartMinimizedToTray = StartMinimizedToTray;
        bool previousMinimizeToTray = MinimizeToTray;

        _quickSwitcherHotkey = CloneHotkey(result.QuickSwitcherHotkey);
        LaunchOnStartup = result.LaunchOnStartup;
        StartMinimizedToTray = result.StartMinimizedToTray;
        MinimizeToTray = result.MinimizeToTray;

        List<Preset> presets = CreatePresets();
        List<string> hotkeyMessages = RefreshHotkeyRegistrations(presets);
        if (ContainsQuickSwitcherRegistrationFailure(hotkeyMessages))
        {
            _quickSwitcherHotkey = previousQuickSwitcherHotkey;
            LaunchOnStartup = previousLaunchOnStartup;
            StartMinimizedToTray = previousStartMinimizedToTray;
            MinimizeToTray = previousMinimizeToTray;
            RefreshHotkeyRegistrations(presets);
            StatusMessage = $"Settings not saved. {hotkeyMessages[0]}";
            return;
        }

        try
        {
            _startupRegistrationService.SetLaunchOnStartup(LaunchOnStartup, StartMinimizedToTray);
        }
        catch (Exception ex)
        {
            _quickSwitcherHotkey = previousQuickSwitcherHotkey;
            LaunchOnStartup = previousLaunchOnStartup;
            StartMinimizedToTray = previousStartMinimizedToTray;
            MinimizeToTray = previousMinimizeToTray;
            RefreshHotkeyRegistrations(presets);
            StatusMessage = $"Settings not saved. Startup registration failed: {ex.Message}";
            return;
        }

        if (!await TrySaveSettingsAsync("Settings were not saved.", presets))
        {
            return;
        }

        StatusMessage = BuildStatusWithHotkeySummary("Settings saved.", presets, hotkeyMessages);
        OnPropertyChanged(nameof(QuickSwitcherHotkeySummary));
        RefreshDiagnostics();
    }

    private async Task ShowOnboardingAsync()
    {
        OnboardingDialogAction action = _onboardingDialogService.ShowOnboarding();
        if (action == OnboardingDialogAction.None)
        {
            return;
        }

        if (!_hasSeenFirstRunOnboarding)
        {
            _hasSeenFirstRunOnboarding = true;
            if (!await TrySaveSettingsAsync("Onboarding state was not saved."))
            {
                return;
            }
        }

        switch (action)
        {
            case OnboardingDialogAction.OpenSettings:
                await OpenSettingsAsync();
                break;
            case OnboardingDialogAction.OpenChromeCaptureSetup:
                OpenChromeCaptureSetup();
                break;
        }
    }

    private List<Preset> CreatePresets()
    {
        return Presets.Select(preset => preset.ToPreset()).ToList();
    }

    private List<string> RefreshHotkeyRegistrations(IReadOnlyList<Preset> presets)
    {
        List<string> messages = [];
        _hotkeyService.UnregisterAll();
        HotkeyValidationResult quickSwitcherValidation = HotkeyValidator.ValidateQuickSwitcherHotkey(
            _quickSwitcherHotkey,
            presets);
        messages.AddRange(quickSwitcherValidation.Errors.Select(error => error.Message));

        if (!quickSwitcherValidation.Errors.Any(error => string.IsNullOrWhiteSpace(error.PresetId)))
        {
            HotkeyRegistrationResult commandPaletteResult = _hotkeyService.RegisterCommandPaletteHotkey(_quickSwitcherHotkey);
            AddRegistrationFailure(messages, commandPaletteResult, "Quick Switcher");
        }

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

        _lastHotkeyMessages = messages.ToList();
        QuickSwitcherHotkeyStatus = BuildQuickSwitcherHotkeyStatus(quickSwitcherValidation, messages);
        return messages;
    }

    private void RefreshDiagnostics()
    {
        ChromeCaptureStatus = _chromeCaptureDiagnosticsService.GetStatusText();
        DiagnosticsText = string.Join(
            Environment.NewLine,
            [
                $"Quick Switcher hotkey: {QuickSwitcherHotkeySummary} - {QuickSwitcherHotkeyStatus}",
                $"Single instance: {SingleInstanceStatus}",
                $"Tray: {TrayStatus}",
                ChromeCaptureStatus,
                $"Last operation: {(string.IsNullOrWhiteSpace(StatusMessage) ? "None." : StatusMessage)}"
            ]);
    }

    private static string BuildQuickSwitcherHotkeyStatus(
        HotkeyValidationResult quickSwitcherValidation,
        IReadOnlyList<string> registrationMessages)
    {
        if (quickSwitcherValidation.Errors.Count > 0)
        {
            return $"failed: {quickSwitcherValidation.Errors[0].Message}";
        }

        string? registrationFailure = registrationMessages.FirstOrDefault(message =>
            message.StartsWith("Quick Switcher hotkey not registered:", StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(registrationFailure))
        {
            return $"failed: {registrationFailure}";
        }

        return "registered.";
    }

    private static HotkeyGesture CloneHotkey(HotkeyGesture hotkey)
    {
        return new HotkeyGesture
        {
            Key = hotkey.Key,
            Ctrl = hotkey.Ctrl,
            Alt = hotkey.Alt,
            Shift = hotkey.Shift,
            Windows = hotkey.Windows
        };
    }

    private static bool ContainsQuickSwitcherRegistrationFailure(IReadOnlyList<string> messages)
    {
        return messages.Any(message => message.StartsWith(
            "Quick Switcher hotkey not registered:",
            StringComparison.OrdinalIgnoreCase));
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
}
