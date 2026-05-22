using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MokoSnap.App.ViewModels;

public sealed class CommandPaletteViewModel : INotifyPropertyChanged
{
    private readonly IReadOnlyList<PresetEditorViewModel> _presets;
    private string _searchText = string.Empty;
    private PresetEditorViewModel? _selectedPreset;

    public CommandPaletteViewModel(IReadOnlyList<PresetEditorViewModel> presets)
    {
        _presets = presets;
        RefreshFilteredPresets();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<PresetEditorViewModel> FilteredPresets { get; } = [];

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
            {
                return;
            }

            _searchText = value;
            OnPropertyChanged();
            RefreshFilteredPresets();
        }
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
        }
    }

    public void SelectNext()
    {
        if (FilteredPresets.Count == 0)
        {
            SelectedPreset = null;
            return;
        }

        int index = SelectedPreset is null ? -1 : FilteredPresets.IndexOf(SelectedPreset);
        SelectedPreset = FilteredPresets[Math.Min(index + 1, FilteredPresets.Count - 1)];
    }

    public void SelectPrevious()
    {
        if (FilteredPresets.Count == 0)
        {
            SelectedPreset = null;
            return;
        }

        int index = SelectedPreset is null ? FilteredPresets.Count : FilteredPresets.IndexOf(SelectedPreset);
        SelectedPreset = FilteredPresets[Math.Max(index - 1, 0)];
    }

    private void RefreshFilteredPresets()
    {
        string query = SearchText.Trim();
        List<PresetEditorViewModel> matches = _presets
            .Where(preset => string.IsNullOrWhiteSpace(query) ||
                preset.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(preset => preset.Name)
            .ToList();

        FilteredPresets.Clear();
        foreach (PresetEditorViewModel preset in matches)
        {
            FilteredPresets.Add(preset);
        }

        SelectedPreset = FilteredPresets.FirstOrDefault();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
