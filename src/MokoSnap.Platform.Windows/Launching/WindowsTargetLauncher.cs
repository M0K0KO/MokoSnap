using MokoSnap.Core.Models;
using MokoSnap.Core.Running;

namespace MokoSnap.Platform.Windows.Launching;

public sealed class WindowsTargetLauncher : ITargetLauncher
{
    private readonly WindowsTargetCommandBuilder _commandBuilder;
    private readonly IWindowsProcessStarter _processStarter;

    public WindowsTargetLauncher(
        WindowsTargetCommandBuilder? commandBuilder = null,
        IWindowsProcessStarter? processStarter = null)
    {
        _commandBuilder = commandBuilder ?? new WindowsTargetCommandBuilder();
        _processStarter = processStarter ?? new WindowsProcessStarter();
    }

    public Task<TargetRunResult> LaunchAsync(TargetConfig target, CancellationToken cancellationToken = default)
    {
        try
        {
            if (target.Type == TargetType.Notion && target.PreferDesktopApp)
            {
                LaunchNotionTarget(target, cancellationToken);
            }
            else
            {
                LaunchCommands(_commandBuilder.BuildCommands(target), cancellationToken);
            }

            return Task.FromResult(TargetRunResult.Successful(target));
        }
        catch (Exception ex)
        {
            return Task.FromResult(TargetRunResult.Failed(target, ex.Message));
        }
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
            _processStarter.Start(command);
        }
    }
}
