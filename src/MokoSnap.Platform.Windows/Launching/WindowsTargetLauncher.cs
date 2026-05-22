using MokoSnap.Core.Models;
using MokoSnap.Core.Running;
using MokoSnap.Platform.Windows.Placement;

namespace MokoSnap.Platform.Windows.Launching;

public sealed class WindowsTargetLauncher : ITargetLauncher
{
    private readonly WindowsTargetCommandBuilder _commandBuilder;
    private readonly IWindowsProcessStarter _processStarter;
    private readonly IWindowsWindowPlacementService _placementService;

    public WindowsTargetLauncher(
        WindowsTargetCommandBuilder? commandBuilder = null,
        IWindowsProcessStarter? processStarter = null,
        IWindowsWindowPlacementService? placementService = null)
    {
        _commandBuilder = commandBuilder ?? new WindowsTargetCommandBuilder();
        _processStarter = processStarter ?? new WindowsProcessStarter();
        _placementService = placementService ?? new WindowsWindowPlacementService();
    }

    public async Task<TargetRunResult> LaunchAsync(TargetConfig target, CancellationToken cancellationToken = default)
    {
        try
        {
            if (target.Type == TargetType.Application)
            {
                return await LaunchApplicationTargetAsync(target, cancellationToken);
            }

            if (target.Type == TargetType.Notion && target.PreferDesktopApp)
            {
                LaunchNotionTarget(target, cancellationToken);
            }
            else
            {
                LaunchCommands(_commandBuilder.BuildCommands(target), cancellationToken);
            }

            return TargetRunResult.Successful(target);
        }
        catch (Exception ex)
        {
            return TargetRunResult.Failed(target, ex.Message);
        }
    }

    private async Task<TargetRunResult> LaunchApplicationTargetAsync(
        TargetConfig target,
        CancellationToken cancellationToken)
    {
        WindowsLaunchCommand command = _commandBuilder.BuildCommands(target).Single();
        int? processId = _processStarter.Start(command);

        if (target.WindowPlacement is null || !target.WindowPlacement.Enabled)
        {
            return TargetRunResult.Successful(target);
        }

        WindowPlacementRestoreResult restoreResult = await _placementService.RestorePlacementAsync(
            target,
            processId,
            cancellationToken);

        return restoreResult.Succeeded
            ? TargetRunResult.Successful(target)
            : TargetRunResult.Successful(target, $"Warning: {restoreResult.Message}");
    }

    private void LaunchNotionTarget(TargetConfig target, CancellationToken cancellationToken)
    {
        IReadOnlyList<WindowsLaunchCommand> desktopCommands = _commandBuilder.BuildNotionDesktopCommands(target);
        try
        {
            LaunchCommands(desktopCommands, cancellationToken);
        }
        catch
        {
            LaunchCommands(_commandBuilder.BuildCommands(target), cancellationToken);
        }
    }

    private void LaunchCommands(
        IReadOnlyList<WindowsLaunchCommand> commands,
        CancellationToken cancellationToken)
    {
        foreach (WindowsLaunchCommand command in commands)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = _processStarter.Start(command);
        }
    }
}
