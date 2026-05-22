using System.Windows;
using MokoSnap.App.ViewModels;
using MokoSnap.App.Views;

namespace MokoSnap.App.Services;

public sealed class CommandPaletteService : ICommandPaletteService
{
    private readonly Window _owner;

    public CommandPaletteService(Window owner)
    {
        _owner = owner;
    }

    public PresetEditorViewModel? SelectPreset(IReadOnlyList<PresetEditorViewModel> presets)
    {
        CommandPaletteViewModel viewModel = new(presets);
        CommandPaletteWindow window = new()
        {
            Owner = _owner,
            DataContext = viewModel
        };

        bool? result = window.ShowDialog();
        return result == true ? viewModel.SelectedPreset : null;
    }
}
