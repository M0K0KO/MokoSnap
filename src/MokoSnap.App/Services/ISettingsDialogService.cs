using MokoSnap.App.ViewModels;

namespace MokoSnap.App.Services;

public interface ISettingsDialogService
{
    SettingsDialogResult? ShowSettings(SettingsDialogRequest request);
}
