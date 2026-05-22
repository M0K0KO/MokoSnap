namespace MokoSnap.Core.Startup;

public static class StartupCommand
{
    public static string Build(string executablePath, bool startMinimized)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            throw new ArgumentException("Executable path is required.", nameof(executablePath));
        }

        string command = $"\"{executablePath}\"";
        return startMinimized ? $"{command} --minimized" : command;
    }
}
