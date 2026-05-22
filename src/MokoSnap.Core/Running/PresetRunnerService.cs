using MokoSnap.Core.Models;

namespace MokoSnap.Core.Running;

public sealed class PresetRunnerService
{
    private readonly ITargetLauncher _targetLauncher;
    private readonly IVisibleWindowCloser _visibleWindowCloser;
    private readonly ILaunchDelay _launchDelay;

    public PresetRunnerService(
        ITargetLauncher targetLauncher,
        IVisibleWindowCloser visibleWindowCloser,
        ILaunchDelay launchDelay)
    {
        _targetLauncher = targetLauncher;
        _visibleWindowCloser = visibleWindowCloser;
        _launchDelay = launchDelay;
    }

    public async Task<PresetRunResult> RunAsync(Preset preset, CancellationToken cancellationToken = default)
    {
        CloseWindowsResult? closeResult = null;
        if (preset.ClosePolicy == ClosePolicy.CloseVisibleWindowsOnly)
        {
            closeResult = await CloseVisibleWindowsAsync(cancellationToken);
        }

        List<TargetRunResult> targetResults = [];
        foreach (TargetConfig target in preset.Targets)
        {
            if (target.LaunchDelayMs > 0)
            {
                await _launchDelay.DelayAsync(TimeSpan.FromMilliseconds(target.LaunchDelayMs), cancellationToken);
            }

            targetResults.Add(await LaunchTargetAsync(target, cancellationToken));
        }

        return new PresetRunResult
        {
            PresetId = preset.Id,
            PresetName = preset.Name,
            CloseWindowsResult = closeResult,
            TargetResults = targetResults,
            Succeeded = (closeResult?.Succeeded ?? true) && targetResults.All(result => result.Succeeded)
        };
    }

    private async Task<CloseWindowsResult> CloseVisibleWindowsAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _visibleWindowCloser.CloseVisibleWindowsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return new CloseWindowsResult
            {
                Succeeded = false,
                Message = ex.Message
            };
        }
    }

    private async Task<TargetRunResult> LaunchTargetAsync(TargetConfig target, CancellationToken cancellationToken)
    {
        try
        {
            return await _targetLauncher.LaunchAsync(target, cancellationToken);
        }
        catch (Exception ex)
        {
            return TargetRunResult.Failed(target, ex.Message);
        }
    }
}
