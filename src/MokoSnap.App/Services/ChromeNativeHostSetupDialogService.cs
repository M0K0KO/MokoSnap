using System.Windows;
using MokoSnap.App.ViewModels;
using MokoSnap.App.Views;
using MokoSnap.Platform.Windows.ChromeCapture;

namespace MokoSnap.App.Services;

public sealed class ChromeNativeHostSetupDialogService : IChromeNativeHostSetupDialogService
{
    private readonly Window _owner;
    private readonly ChromeNativeHostSetupService _setupService;

    public ChromeNativeHostSetupDialogService(Window owner, ChromeNativeHostSetupService setupService)
    {
        _owner = owner;
        _setupService = setupService;
    }

    public void ShowSetupDialog()
    {
        ChromeNativeHostSetupDialogViewModel viewModel = new(_setupService);
        ChromeNativeHostSetupDialog dialog = new()
        {
            Owner = _owner,
            DataContext = viewModel
        };
        dialog.ShowDialog();
    }
}
