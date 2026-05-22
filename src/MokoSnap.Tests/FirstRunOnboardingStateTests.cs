using MokoSnap.Core.Models;
using MokoSnap.Core.Onboarding;

namespace MokoSnap.Tests;

public class FirstRunOnboardingStateTests
{
    [Fact]
    public void ShouldShowWhenNotSeenAndNotStartingMinimized()
    {
        Assert.True(FirstRunOnboardingState.ShouldShow(false, false));
    }

    [Fact]
    public void ShouldNotShowWhenAlreadySeen()
    {
        Assert.False(FirstRunOnboardingState.ShouldShow(true, false));
    }

    [Fact]
    public void ShouldNotShowWhenStartingMinimizedToTray()
    {
        Assert.False(FirstRunOnboardingState.ShouldShow(false, true));
    }

    [Fact]
    public void MarkSeenUpdatesSettings()
    {
        AppSettings settings = AppSettings.CreateDefault();

        FirstRunOnboardingState.MarkSeen(settings);

        Assert.True(settings.HasSeenFirstRunOnboarding);
    }
}
