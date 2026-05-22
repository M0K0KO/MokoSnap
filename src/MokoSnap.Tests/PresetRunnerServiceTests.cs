using MokoSnap.Core.Models;
using MokoSnap.Core.Running;

namespace MokoSnap.Tests;

public class PresetRunnerServiceTests
{
    [Fact]
    public async Task NoneClosePolicyDoesNotCallWindowCloser()
    {
        FakeTargetLauncher launcher = new();
        FakeVisibleWindowCloser closer = new();
        FakeLaunchDelay delay = new();
        PresetRunnerService runner = new(launcher, closer, delay);

        PresetRunResult result = await runner.RunAsync(new Preset
        {
            ClosePolicy = ClosePolicy.None,
            Targets = [CreateTarget("one")]
        });

        Assert.True(result.Succeeded);
        Assert.Equal(0, closer.CallCount);
        Assert.Null(result.CloseWindowsResult);
    }

    [Fact]
    public async Task CloseVisibleWindowsOnlyCallsWindowCloserBeforeLaunchingTargets()
    {
        List<string> calls = [];
        FakeTargetLauncher launcher = new(calls);
        FakeVisibleWindowCloser closer = new(calls);
        FakeLaunchDelay delay = new();
        PresetRunnerService runner = new(launcher, closer, delay);

        PresetRunResult result = await runner.RunAsync(new Preset
        {
            ClosePolicy = ClosePolicy.CloseVisibleWindowsOnly,
            Targets = [CreateTarget("one")]
        });

        Assert.True(result.Succeeded);
        Assert.Equal(1, closer.CallCount);
        Assert.NotNull(result.CloseWindowsResult);
        Assert.Equal(["close", "launch:one"], calls);
    }

    [Fact]
    public async Task TargetsLaunchInPresetOrder()
    {
        FakeTargetLauncher launcher = new();
        FakeVisibleWindowCloser closer = new();
        FakeLaunchDelay delay = new();
        PresetRunnerService runner = new(launcher, closer, delay);

        await runner.RunAsync(new Preset
        {
            Targets =
            [
                CreateTarget("one"),
                CreateTarget("two"),
                CreateTarget("three")
            ]
        });

        Assert.Equal(["one", "two", "three"], launcher.LaunchedTargetNames);
    }

    [Fact]
    public async Task FailedTargetDoesNotStopLaterTargets()
    {
        FakeTargetLauncher launcher = new();
        launcher.Failures.Add("two", "target failed");
        FakeVisibleWindowCloser closer = new();
        FakeLaunchDelay delay = new();
        PresetRunnerService runner = new(launcher, closer, delay);

        PresetRunResult result = await runner.RunAsync(new Preset
        {
            Targets =
            [
                CreateTarget("one"),
                CreateTarget("two"),
                CreateTarget("three")
            ]
        });

        Assert.False(result.Succeeded);
        Assert.Equal(["one", "two", "three"], launcher.LaunchedTargetNames);
        Assert.Equal([true, false, true], result.TargetResults.Select(target => target.Succeeded).ToArray());
        Assert.Equal("target failed", result.TargetResults[1].Message);
    }

    [Fact]
    public async Task LaunchDelayIsInjectedAndRecordedBeforeTargetLaunch()
    {
        List<string> calls = [];
        FakeTargetLauncher launcher = new(calls);
        FakeVisibleWindowCloser closer = new(calls);
        FakeLaunchDelay delay = new(calls);
        PresetRunnerService runner = new(launcher, closer, delay);

        await runner.RunAsync(new Preset
        {
            Targets =
            [
                CreateTarget("one", launchDelayMs: 10),
                CreateTarget("two", launchDelayMs: 25)
            ]
        });

        Assert.Equal([10, 25], delay.Delays.Select(item => (int)item.TotalMilliseconds).ToArray());
        Assert.Equal(["delay:10", "launch:one", "delay:25", "launch:two"], calls);
    }

    private static TargetConfig CreateTarget(string displayName, int launchDelayMs = 0)
    {
        return new TargetConfig
        {
            Type = TargetType.Url,
            DisplayName = displayName,
            Url = $"https://example.com/{displayName}",
            LaunchDelayMs = launchDelayMs
        };
    }

    private sealed class FakeTargetLauncher : ITargetLauncher
    {
        private readonly List<string>? _calls;

        public FakeTargetLauncher(List<string>? calls = null)
        {
            _calls = calls;
        }

        public List<string> LaunchedTargetNames { get; } = [];

        public Dictionary<string, string> Failures { get; } = [];

        public Task<TargetRunResult> LaunchAsync(TargetConfig target, CancellationToken cancellationToken = default)
        {
            LaunchedTargetNames.Add(target.DisplayName);
            _calls?.Add($"launch:{target.DisplayName}");

            if (Failures.TryGetValue(target.DisplayName, out string? message))
            {
                return Task.FromResult(TargetRunResult.Failed(target, message));
            }

            return Task.FromResult(TargetRunResult.Successful(target));
        }
    }

    private sealed class FakeVisibleWindowCloser : IVisibleWindowCloser
    {
        private readonly List<string>? _calls;

        public FakeVisibleWindowCloser(List<string>? calls = null)
        {
            _calls = calls;
        }

        public int CallCount { get; private set; }

        public Task<CloseWindowsResult> CloseVisibleWindowsAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;
            _calls?.Add("close");
            return Task.FromResult(new CloseWindowsResult { Succeeded = true, ClosedWindowCount = 2 });
        }
    }

    private sealed class FakeLaunchDelay : ILaunchDelay
    {
        private readonly List<string>? _calls;

        public FakeLaunchDelay(List<string>? calls = null)
        {
            _calls = calls;
        }

        public List<TimeSpan> Delays { get; } = [];

        public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
        {
            Delays.Add(delay);
            _calls?.Add($"delay:{(int)delay.TotalMilliseconds}");
            return Task.CompletedTask;
        }
    }
}
