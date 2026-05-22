using MokoSnap.Core.Models;

namespace MokoSnap.Core.Running;

public interface ITargetLauncher
{
    Task<TargetRunResult> LaunchAsync(TargetConfig target, CancellationToken cancellationToken = default);
}

public interface IVisibleWindowCloser
{
    Task<CloseWindowsResult> CloseVisibleWindowsAsync(CancellationToken cancellationToken = default);
}

public interface ILaunchDelay
{
    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default);
}

public sealed class SystemLaunchDelay : ILaunchDelay
{
    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        return Task.Delay(delay, cancellationToken);
    }
}
