using MokoSnap.Core.Models;

namespace MokoSnap.Core.Onboarding;

public static class FirstRunOnboardingState
{
    public static bool ShouldShow(bool hasSeenFirstRunOnboarding, bool startsMinimizedToTray)
    {
        return !hasSeenFirstRunOnboarding && !startsMinimizedToTray;
    }

    public static void MarkSeen(AppSettings settings)
    {
        settings.HasSeenFirstRunOnboarding = true;
    }
}
