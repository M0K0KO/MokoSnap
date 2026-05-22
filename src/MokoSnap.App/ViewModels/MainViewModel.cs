using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MokoSnap.App.Services;
using MokoSnap.Core.Models;
using MokoSnap.Core.Storage;

namespace MokoSnap.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly IJsonStorage<AppSettings> _settingsStorage;
    private readonly IConfirmationService _confirmationService;
    private PresetEditorViewModel? _selectedPreset;
    private string _statusMessage = string.Empty;

    public MainViewModel(
        IJsonStorage<AppSettings> settingsStorage,
        IConfirmationService confirmationService)
    {
        _settingsStorage = settingsStorage;
        _confirmationService = confirmationService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new AsyncRelayCommand(AddAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => SelectedPreset is not null);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedPreset is not null);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<PresetEditorViewModel> Presets { get; } = [];

    public Array ClosePolicies { get; } = Enum.GetValues(typeof(ClosePolicy));

    public Array CloseConfirmationPolicies { get; } = Enum.GetValues(typeof(CloseConfirmationPolicy));

    public AsyncRelayCommand LoadCommand { get; }

    public AsyncRelayCommand AddCommand { get; }

    public AsyncRelayCommand SaveCommand { get; }

    public AsyncRelayCommand DeleteCommand { get; }

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
