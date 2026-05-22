using MokoSnap.App.ViewModels;

namespace MokoSnap.App.Services;

public interface ICommandPaletteService
{
    CommandPaletteSelection SelectPreset(IReadOnlyList<PresetEditorViewModel> presets);
}

public sealed class CommandPaletteSelection
{
    public PresetEditorViewModel? Preset { get; init; }

    public bool OpensSettings { get; init; }
}
