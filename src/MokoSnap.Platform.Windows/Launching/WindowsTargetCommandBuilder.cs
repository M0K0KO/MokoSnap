using MokoSnap.Core.Models;

namespace MokoSnap.Platform.Windows.Launching;

public sealed class WindowsTargetCommandBuilder
{
    private readonly IChromeExecutableResolver _chromeExecutableResolver;

    public WindowsTargetCommandBuilder(IChromeExecutableResolver? chromeExecutableResolver = null)
    {
        _chromeExecutableResolver = chromeExecutableResolver ?? new ChromeExecutableResolver();
    }

    public IReadOnlyList<WindowsLaunchCommand> BuildCommands(TargetConfig target)
    {
        return target.Type switch
        {
            TargetType.Application => [BuildApplicationCommand(target)],
            TargetType.Url => [BuildShellCommand(target.Url)],
            TargetType.Folder => [BuildShellCommand(target.Path)],
            TargetType.Chrome => BuildChromeCommands(target),
            TargetType.Notion => BuildNotionFallbackCommands(target),
            _ => throw new NotSupportedException($"Unsupported target type: {target.Type}")
        };
    }

    public IReadOnlyList<WindowsLaunchCommand> BuildNotionDesktopCommands(TargetConfig target)
    {
        return target.PageUrls
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Select(url => BuildShellCommand(ToNotionDesktopUrl(url)))
            .ToList();
    }

    private static WindowsLaunchCommand BuildApplicationCommand(TargetConfig target)
    {
        if (string.IsNullOrWhiteSpace(target.ExecutablePath))
        {
            throw new ArgumentException("Application target requires an executable path.", nameof(target));
        }

        return new WindowsLaunchCommand
        {
            FileName = target.ExecutablePath,
            Arguments = target.Arguments,
            WorkingDirectory = target.WorkingDirectory,
            UseShellExecute = target.RunAsAdmin,
            Verb = target.RunAsAdmin ? "runas" : string.Empty
        };
    }

    private IReadOnlyList<WindowsLaunchCommand> BuildChromeCommands(TargetConfig target)
    {
        List<string> urls = target.Urls
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .ToList();

        string? chromePath = _chromeExecutableResolver.ResolveChromeExecutablePath();
        if (string.IsNullOrWhiteSpace(chromePath))
        {
            return urls.Select(BuildShellCommand).ToList();
        }

        List<string> arguments = [];
        if (target.OpenInNewWindow)
        {
            arguments.Add("--new-window");
        }

        if (!string.IsNullOrWhiteSpace(target.ProfileName))
        {
            arguments.Add($"--profile-directory={target.ProfileName}");
        }

        arguments.AddRange(urls);

        return
        [
            new WindowsLaunchCommand
            {
                FileName = chromePath,
                ArgumentList = arguments,
                UseShellExecute = false
            }
        ];
    }

    private static IReadOnlyList<WindowsLaunchCommand> BuildNotionFallbackCommands(TargetConfig target)
    {
        return target.PageUrls
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Select(BuildShellCommand)
            .ToList();
    }

    private static WindowsLaunchCommand BuildShellCommand(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Shell target requires a path or URL.", nameof(value));
        }

        return new WindowsLaunchCommand
        {
            FileName = value,
            UseShellExecute = true
        };
    }

    private static string ToNotionDesktopUrl(string pageUrl)
    {
        if (Uri.TryCreate(pageUrl, UriKind.Absolute, out Uri? uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return $"notion://{uri.Host}{uri.PathAndQuery}";
        }

        return pageUrl;
    }
}
