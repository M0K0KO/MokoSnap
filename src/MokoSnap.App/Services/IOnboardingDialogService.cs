namespace MokoSnap.App.Services;

public interface IOnboardingDialogService
{
    OnboardingDialogAction ShowOnboarding();
}

public enum OnboardingDialogAction
{
    None,
    GetStarted,
    OpenSettings,
    OpenChromeCaptureSetup
}
