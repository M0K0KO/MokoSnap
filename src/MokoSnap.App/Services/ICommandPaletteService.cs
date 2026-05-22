using MokoSnap.App.ViewModels;

namespace MokoSnap.App.Services;

public interface ICommandPaletteService
{
    PresetEditorViewModel? SelectPreset(IReadOnlyList<PresetEditorViewModel> presets);
}
