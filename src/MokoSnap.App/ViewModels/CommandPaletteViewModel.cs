using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MokoSnap.App.ViewModels;

public sealed class CommandPaletteViewModel : INotifyPropertyChanged
{
    private readonly IReadOnlyList<PresetEditorViewModel> _presets;
    private string _searchText = string.Empty;
    private CommandPaletteItemViewModel? _selectedItem;

    public CommandPaletteViewModel(IReadOnlyList<PresetEditorViewModel> presets)
    {
        _presets = presets;
        RefreshFilteredPresets();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<CommandPaletteItemViewModel> FilteredItems { get; } = [];

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

    public CommandPaletteItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem == value)
            {
                return;
            }

            _selectedItem = value;
            OnPropertyChanged();
        }
    }

    public void SelectNext()
    {
        if (FilteredItems.Count == 0)
        {
            SelectedItem = null;
            return;
        }

        int index = SelectedItem is null ? -1 : FilteredItems.IndexOf(SelectedItem);
        SelectedItem = FilteredItems[Math.Min(index + 1, FilteredItems.Count - 1)];
    }

    public void SelectPrevious()
    {
        if (FilteredItems.Count == 0)
        {
            SelectedItem = null;
            return;
        }

        int index = SelectedItem is null ? FilteredItems.Count : FilteredItems.IndexOf(SelectedItem);
        SelectedItem = FilteredItems[Math.Max(index - 1, 0)];
    }

    private void RefreshFilteredPresets()
    {
        string query = SearchText.Trim();
        List<CommandPaletteItemViewModel> matches = [];
        if (string.IsNullOrWhiteSpace(query) ||
            "settings".Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            matches.Add(CommandPaletteItemViewModel.OpenSettings());
        }

        matches.AddRange(_presets
            .Where(preset => string.IsNullOrWhiteSpace(query) ||
                preset.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(preset => preset.Name)
            .Select(CommandPaletteItemViewModel.RunPreset));

        FilteredItems.Clear();
        foreach (CommandPaletteItemViewModel item in matches)
        {
            FilteredItems.Add(item);
        }

        SelectedItem = FilteredItems.FirstOrDefault();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed class CommandPaletteItemViewModel
{
    private CommandPaletteItemViewModel(string name, PresetEditorViewModel? preset, bool opensSettings)
    {
        Name = name;
        Preset = preset;
        OpensSettings = opensSettings;
    }

    public string Name { get; }

    public PresetEditorViewModel? Preset { get; }

    public bool OpensSettings { get; }

    public static CommandPaletteItemViewModel OpenSettings()
    {
        return new CommandPaletteItemViewModel("Settings", null, true);
    }

    public static CommandPaletteItemViewModel RunPreset(PresetEditorViewModel preset)
    {
        return new CommandPaletteItemViewModel(preset.Name, preset, false);
    }
}
