namespace MokoSnap.Platform.Windows.Launching;

public sealed class WindowsLaunchCommand
{
    public string FileName { get; init; } = string.Empty;

    public string Arguments { get; init; } = string.Empty;

    public List<string> ArgumentList { get; init; } = [];

    public string WorkingDirectory { get; init; } = string.Empty;

    public bool UseShellExecute { get; init; }

    public string Verb { get; init; } = string.Empty;
}
