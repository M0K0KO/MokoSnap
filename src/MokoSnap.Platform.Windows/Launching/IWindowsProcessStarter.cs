namespace MokoSnap.Platform.Windows.Launching;

public interface IWindowsProcessStarter
{
    void Start(WindowsLaunchCommand command);
}
