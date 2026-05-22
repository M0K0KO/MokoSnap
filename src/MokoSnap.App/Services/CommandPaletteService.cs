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

    public CommandPaletteSelection SelectPreset(IReadOnlyList<PresetEditorViewModel> presets)
    {
        CommandPaletteViewModel viewModel = new(presets);
        CommandPaletteWindow window = new()
        {
            Owner = _owner,
            DataContext = viewModel
        };

        bool? result = window.ShowDialog();
        if (result != true || viewModel.SelectedItem is null)
        {
            return new CommandPaletteSelection();
        }

        if (viewModel.SelectedItem.OpensSettings)
        {
            return new CommandPaletteSelection { OpensSettings = true };
        }

        return new CommandPaletteSelection { Preset = viewModel.SelectedItem.Preset };
    }
}
