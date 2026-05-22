using MokoSnap.Core.Models;
using MokoSnap.Platform.Windows.Launching;

namespace MokoSnap.Tests;

public class WindowsTargetLauncherTests
{
    [Fact]
    public async Task LaunchAsyncUsesProcessStarterWithoutStartingRealProcesses()
    {
        FakeProcessStarter starter = new();
        WindowsTargetLauncher launcher = new(
            new WindowsTargetCommandBuilder(new FakeChromeExecutableResolver(null)),
            starter);

        var result = await launcher.LaunchAsync(new TargetConfig
        {
            Type = TargetType.Url,
            Url = "https://example.com"
        });

        Assert.True(result.Succeeded);
        WindowsLaunchCommand command = Assert.Single(starter.Commands);
        Assert.Equal("https://example.com", command.FileName);
        Assert.True(command.UseShellExecute);
    }

    [Fact]
    public async Task NotionTargetFallsBackToPageUrlWhenDesktopLaunchFails()
    {
        FakeProcessStarter starter = new();
        starter.FailFirstStart = true;
        WindowsTargetLauncher launcher = new(
            new WindowsTargetCommandBuilder(new FakeChromeExecutableResolver(null)),
            starter);

        var result = await launcher.LaunchAsync(new TargetConfig
        {
            Type = TargetType.Notion,
            PreferDesktopApp = true,
            PageUrls = ["https://www.notion.so/workspace/page-id"]
        });

        Assert.True(result.Succeeded);
        Assert.Equal(2, starter.Commands.Count);
        Assert.Equal("notion://www.notion.so/workspace/page-id", starter.Commands[0].FileName);
        Assert.Equal("https://www.notion.so/workspace/page-id", starter.Commands[1].FileName);
    }

    private sealed class FakeProcessStarter : IWindowsProcessStarter
    {
        public List<WindowsLaunchCommand> Commands { get; } = [];

        public bool FailFirstStart { get; set; }

        public void Start(WindowsLaunchCommand command)
        {
            Commands.Add(command);
            if (FailFirstStart)
            {
                FailFirstStart = false;
                throw new InvalidOperationException("launch failed");
            }
        }
    }

    private sealed class FakeChromeExecutableResolver : IChromeExecutableResolver
    {
        private readonly string? _path;

        public FakeChromeExecutableResolver(string? path)
        {
            _path = path;
        }

        public string? ResolveChromeExecutablePath()
        {
            return _path;
        }
    }
}
