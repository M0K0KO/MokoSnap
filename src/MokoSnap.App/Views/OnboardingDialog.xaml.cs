using System.ComponentModel;
using System.Windows;
using MokoSnap.App.Services;

namespace MokoSnap.App.Views;

public partial class OnboardingDialog : Window
{
    public OnboardingDialog()
    {
        InitializeComponent();
    }

    public OnboardingDialogAction Result { get; private set; } = OnboardingDialogAction.None;

    protected override void OnClosing(CancelEventArgs e)
    {
        if (Result == OnboardingDialogAction.None)
        {
            Result = OnboardingDialogAction.GetStarted;
        }

        base.OnClosing(e);
    }

    private void OnGetStartedClicked(object sender, RoutedEventArgs e)
    {
        CloseWithResult(OnboardingDialogAction.GetStarted);
    }

    private void OnDoNotShowAgainClicked(object sender, RoutedEventArgs e)
    {
        CloseWithResult(OnboardingDialogAction.GetStarted);
    }

    private void OnOpenSettingsClicked(object sender, RoutedEventArgs e)
    {
        CloseWithResult(OnboardingDialogAction.OpenSettings);
    }

    private void OnOpenChromeCaptureSetupClicked(object sender, RoutedEventArgs e)
    {
        CloseWithResult(OnboardingDialogAction.OpenChromeCaptureSetup);
    }

    private void CloseWithResult(OnboardingDialogAction action)
    {
        Result = action;
        DialogResult = true;
    }
}
