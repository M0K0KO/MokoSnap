using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MokoSnap.App.Services;
using MokoSnap.Core.Capture;
using MokoSnap.Core.Models;
using MokoSnap.Core.Running;
using MokoSnap.Core.Storage;

namespace MokoSnap.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly IJsonStorage<AppSettings> _settingsStorage;
    private readonly IConfirmationService _confirmationService;
    private readonly ICapturedAppSelectionService _capturedAppSelectionService;
    private readonly PresetRunnerService _presetRunnerService;
    private PresetEditorViewModel? _selectedPreset;
    private string _statusMessage = string.Empty;
    private string _runResultMessage = string.Empty;
    private bool _isRunning;

    public MainViewModel(
        IJsonStorage<AppSettings> settingsStorage,
        IConfirmationService confirmationService,
        ICapturedAppSelectionService capturedAppSelectionService,
        PresetRunnerService presetRunnerService)
    {
        _settingsStorage = settingsStorage;
        _confirmationService = confirmationService;
        _capturedAppSelectionService = capturedAppSelectionService;
        _presetRunnerService = presetRunnerService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new AsyncRelayCommand(AddAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => SelectedPreset is not null);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedPreset is not null);
        CaptureCurrentAppsCommand = new AsyncRelayCommand(CaptureCurrentAppsAsync, () => SelectedPreset is not null);
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

    private async Task LoadAsync()
    {
        AppSettings settings = await _settingsStorage.LoadAsync();
        Presets.Clear();

        foreach (Preset preset in settings.Presets)
        {
            Presets.Add(new PresetEditorViewModel(preset));
        }

        SelectedPreset = Presets.FirstOrDefault();
        StatusMessage = Presets.Count == 0 ? "No presets yet." : $"Loaded {Presets.Count} preset(s).";
    }

    private async Task AddAsync()
    {
        PresetEditorViewModel preset = new(new Preset
        {
            Name = "New Preset"
        });

        Presets.Add(preset);
        SelectedPreset = preset;
        await SaveSettingsAsync();
        StatusMessage = "Preset added.";
    }

    private async Task SaveAsync()
    {
        if (SelectedPreset is null)
        {
            return;
        }

        await SaveSettingsAsync();
        StatusMessage = "Preset saved.";
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

        await SaveSettingsAsync();
        StatusMessage = "Preset deleted.";
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
        await SaveSettingsAsync();
        StatusMessage = $"Captured {selectedApps.Count} app target(s).";
    }

    private async Task RunAsync()
    {
        if (SelectedPreset is null)
        {
            return;
        }

        Preset preset = SelectedPreset.ToPreset();
        if (preset.ClosePolicy == ClosePolicy.CloseVisibleWindowsOnly)
        {
            RunResultMessage = "Close visible windows is not implemented yet. Set close policy to None or implement the close windows task first.";
            StatusMessage = "Preset was not run.";
            return;
        }

        try
        {
            IsRunning = true;
            StatusMessage = "Running preset...";
            RunResultMessage = string.Empty;

            PresetRunResult result = await _presetRunnerService.RunAsync(preset);
            RunResultMessage = FormatRunResult(result);
            StatusMessage = result.Succeeded ? "Preset run completed." : "Preset run completed with failures.";
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
        AppSettings settings = new()
        {
            Presets = Presets.Select(preset => preset.ToPreset()).ToList()
        };

        return _settingsStorage.SaveAsync(settings);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
