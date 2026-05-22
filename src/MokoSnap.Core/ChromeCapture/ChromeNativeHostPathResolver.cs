namespace MokoSnap.Core.ChromeCapture;

public static class ChromeNativeHostPathResolver
{
    public const string NativeHostExeName = "MokoSnap.NativeHost.exe";

    public static string Resolve(
        string appBaseDirectory,
        string? explicitNativeHostExePath,
        Func<string, bool> fileExists)
    {
        if (!string.IsNullOrWhiteSpace(explicitNativeHostExePath))
        {
            return explicitNativeHostExePath;
        }

        List<string> candidates = BuildCandidates(appBaseDirectory);
        return candidates.FirstOrDefault(fileExists) ?? candidates[0];
    }

    public static List<string> BuildCandidates(string appBaseDirectory)
    {
        string baseDirectory = Path.GetFullPath(appBaseDirectory);
        DirectoryInfo baseInfo = new(baseDirectory);
        List<string> candidates =
        [
            Path.Combine(baseInfo.Parent?.FullName ?? baseDirectory, "MokoSnap.NativeHost", NativeHostExeName),
            Path.Combine(baseDirectory, NativeHostExeName),
            Path.Combine(baseDirectory, "MokoSnap.NativeHost", NativeHostExeName)
        ];

        DirectoryInfo? srcDirectory = FindAncestor(baseDirectory, "src");
        if (srcDirectory is not null)
        {
            string nativeHostProject = Path.Combine(srcDirectory.FullName, "MokoSnap.NativeHost");
            candidates.Add(Path.Combine(nativeHostProject, "bin", "Release", "net8.0", NativeHostExeName));
            candidates.Add(Path.Combine(nativeHostProject, "bin", "Debug", "net8.0", NativeHostExeName));
        }

        return candidates
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static DirectoryInfo? FindAncestor(string startPath, string directoryName)
    {
        DirectoryInfo? directory = new(startPath);
        while (directory is not null)
        {
            if (directory.Name.Equals(directoryName, StringComparison.OrdinalIgnoreCase))
            {
                return directory;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
