using MokoSnap.Core.Models;
using MokoSnap.Platform.Windows.Launching;

namespace MokoSnap.Tests;

public class WindowsTargetCommandBuilderTests
{
    [Fact]
    public void ApplicationTargetBuildsExecutableCommand()
    {
        WindowsTargetCommandBuilder builder = new(new FakeChromeExecutableResolver(null));

        WindowsLaunchCommand command = Assert.Single(builder.BuildCommands(new TargetConfig
        {
            Type = TargetType.Application,
            ExecutablePath = @"C:\Tools\app.exe",
            Arguments = "--open file.txt",
            WorkingDirectory = @"C:\Tools",
            RunAsAdmin = true
        }));

        Assert.Equal(@"C:\Tools\app.exe", command.FileName);
        Assert.Equal("--open file.txt", command.Arguments);
        Assert.Equal(@"C:\Tools", command.WorkingDirectory);
        Assert.True(command.UseShellExecute);
        Assert.Equal("runas", command.Verb);
    }

    [Fact]
    public void UrlTargetBuildsShellCommand()
    {
        WindowsTargetCommandBuilder builder = new(new FakeChromeExecutableResolver(null));

        WindowsLaunchCommand command = Assert.Single(builder.BuildCommands(new TargetConfig
        {
            Type = TargetType.Url,
            Url = "https://example.com"
        }));

        Assert.Equal("https://example.com", command.FileName);
        Assert.True(command.UseShellExecute);
    }

    [Fact]
    public void FolderTargetBuildsShellCommand()
    {
        WindowsTargetCommandBuilder builder = new(new FakeChromeExecutableResolver(null));

        WindowsLaunchCommand command = Assert.Single(builder.BuildCommands(new TargetConfig
        {
            Type = TargetType.Folder,
            Path = @"C:\Work"
        }));

        Assert.Equal(@"C:\Work", command.FileName);
        Assert.True(command.UseShellExecute);
    }

    [Fact]
    public void ChromeTargetBuildsChromeCommandWhenChromeIsResolved()
    {
        WindowsTargetCommandBuilder builder = new(new FakeChromeExecutableResolver(@"C:\Chrome\chrome.exe"));

        WindowsLaunchCommand command = Assert.Single(builder.BuildCommands(new TargetConfig
        {
            Type = TargetType.Chrome,
            ProfileName = "Profile 1",
            OpenInNewWindow = true,
            Urls =
            [
                "https://example.com/one",
                "https://example.com/two"
            ]
        }));

        Assert.Equal(@"C:\Chrome\chrome.exe", command.FileName);
        Assert.False(command.UseShellExecute);
        Assert.Equal(
            [
                "--new-window",
                "--profile-directory=Profile 1",
                "https://example.com/one",
                "https://example.com/two"
            ],
            command.ArgumentList);
    }

    [Fact]
    public void ChromeTargetFallsBackToShellOpeningUrlsWhenChromeIsNotResolved()
    {
        WindowsTargetCommandBuilder builder = new(new FakeChromeExecutableResolver(null));

        IReadOnlyList<WindowsLaunchCommand> commands = builder.BuildCommands(new TargetConfig
        {
            Type = TargetType.Chrome,
            Urls =
            [
                "https://example.com/one",
                "https://example.com/two"
            ]
        });

        Assert.Equal(2, commands.Count);
        Assert.All(commands, command => Assert.True(command.UseShellExecute));
        Assert.Equal("https://example.com/one", commands[0].FileName);
        Assert.Equal("https://example.com/two", commands[1].FileName);
    }

    [Fact]
    public void NotionDesktopCommandsUseNotionScheme()
    {
        WindowsTargetCommandBuilder builder = new(new FakeChromeExecutableResolver(null));

        WindowsLaunchCommand command = Assert.Single(builder.BuildNotionDesktopCommands(new TargetConfig
        {
            Type = TargetType.Notion,
            PageUrls = ["https://www.notion.so/workspace/page-id?pvs=4"]
        }));

        Assert.Equal("notion://www.notion.so/workspace/page-id?pvs=4", command.FileName);
        Assert.True(command.UseShellExecute);
    }

    [Fact]
    public void NotionFallbackCommandsShellOpenPageUrls()
    {
        WindowsTargetCommandBuilder builder = new(new FakeChromeExecutableResolver(null));

        WindowsLaunchCommand command = Assert.Single(builder.BuildCommands(new TargetConfig
        {
            Type = TargetType.Notion,
            PageUrls = ["https://www.notion.so/workspace/page-id"]
        }));

        Assert.Equal("https://www.notion.so/workspace/page-id", command.FileName);
        Assert.True(command.UseShellExecute);
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
