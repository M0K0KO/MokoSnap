using System.Diagnostics;

namespace MokoSnap.Platform.Windows.Launching;

public sealed class WindowsProcessStarter : IWindowsProcessStarter
{
    public void Start(WindowsLaunchCommand command)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = command.FileName,
            UseShellExecute = command.UseShellExecute
        };

        if (!string.IsNullOrWhiteSpace(command.WorkingDirectory))
        {
            startInfo.WorkingDirectory = command.WorkingDirectory;
        }

        if (!string.IsNullOrWhiteSpace(command.Verb))
        {
            startInfo.Verb = command.Verb;
        }

        if (command.ArgumentList.Count > 0)
        {
            foreach (string argument in command.ArgumentList)
            {
                startInfo.ArgumentList.Add(argument);
            }
        }
        else if (!string.IsNullOrWhiteSpace(command.Arguments))
        {
            startInfo.Arguments = command.Arguments;
        }

        Process.Start(startInfo);
    }
}
