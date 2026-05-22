using MokoSnap.Core.Startup;

namespace MokoSnap.Tests;

public class StartupCommandTests
{
    [Fact]
    public void BuildQuotesExecutablePath()
    {
        string command = StartupCommand.Build(@"C:\Program Files\MokoSnap\MokoSnap.App.exe", false);

        Assert.Equal(@"""C:\Program Files\MokoSnap\MokoSnap.App.exe""", command);
    }

    [Fact]
    public void BuildAddsMinimizedArgumentWhenRequested()
    {
        string command = StartupCommand.Build(@"C:\Tools\MokoSnap.App.exe", true);

        Assert.Equal(@"""C:\Tools\MokoSnap.App.exe"" --minimized", command);
    }
}
