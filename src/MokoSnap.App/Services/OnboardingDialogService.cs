using System.Windows;
using MokoSnap.App.Views;

namespace MokoSnap.App.Services;

public sealed class OnboardingDialogService : IOnboardingDialogService
{
    private readonly Window _owner;

    public OnboardingDialogService(Window owner)
    {
        _owner = owner;
    }

    public OnboardingDialogAction ShowOnboarding()
    {
        ShowOwnerWindow();
        OnboardingDialog dialog = new()
        {
            Owner = _owner
        };

        dialog.ShowDialog();
        return dialog.Result;
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
