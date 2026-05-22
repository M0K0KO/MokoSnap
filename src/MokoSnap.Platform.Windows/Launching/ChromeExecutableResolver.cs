namespace MokoSnap.Platform.Windows.Launching;

public sealed class ChromeExecutableResolver : IChromeExecutableResolver
{
    public string? ResolveChromeExecutablePath()
    {
        string[] candidates =
        [
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Google",
                "Chrome",
                "Application",
                "chrome.exe"),
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Google",
                "Chrome",
                "Application",
                "chrome.exe"),
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Google",
                "Chrome",
                "Application",
                "chrome.exe")
        ];

        return candidates.FirstOrDefault(File.Exists);
    }
}
