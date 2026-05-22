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
        if (result != true || viewModel.SelectedItem is null)
        {
            return null;
        }

        if (viewModel.SelectedItem.OpensSettings)
        {
            ShowOwnerWindow();
            return null;
        }

        return viewModel.SelectedItem.Preset;
    }

    private void ShowOwnerWindow()
    {
        _owner.Show();
        if (_owner.WindowState == WindowState.Minimized)
        {
            _owner.WindowState = WindowState.Normal;
        }

        DialogFocusHelper.ActivateAndFocus(_owner, _owner);
    }
}
