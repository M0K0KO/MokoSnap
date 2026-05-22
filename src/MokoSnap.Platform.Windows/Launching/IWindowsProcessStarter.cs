namespace MokoSnap.Platform.Windows.Launching;

public interface IWindowsProcessStarter
{
    int? Start(WindowsLaunchCommand command);
}
