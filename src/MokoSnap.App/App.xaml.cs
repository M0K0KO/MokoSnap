using System.Windows;

namespace MokoSnap.App;

public partial class App : System.Windows.Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        MainWindow window = new();
        MainWindow = window;
        bool startMinimized = e.Args.Any(arg => arg.Equals("--minimized", StringComparison.OrdinalIgnoreCase));
        await window.StartAsync(startMinimized);
    }
}
