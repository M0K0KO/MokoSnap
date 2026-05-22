using System.Text.Json;
using MokoSnap.Core.ChromeCapture;
using MokoSnap.Core.Storage;

namespace MokoSnap.Platform.Windows.ChromeCapture;

public sealed class ChromeNativeHostSetupService
{
    private readonly IChromeNativeHostRegistry _registry;
    private readonly string _manifestPath;
    private readonly string _latestCapturePath;
    private readonly string _nativeHostExePath;

    public ChromeNativeHostSetupService(
        IChromeNativeHostRegistry? registry = null,
        string? manifestPath = null,
        string? latestCapturePath = null,
        string? nativeHostExePath = null)
    {
        _registry = registry ?? CreateDefaultRegistry();
        _manifestPath = manifestPath ?? MokoSnapStoragePaths.ChromeNativeHostManifestPath;
        _latestCapturePath = latestCapturePath ?? MokoSnapStoragePaths.ChromeTabsLatestPath;
        _nativeHostExePath = nativeHostExePath ?? LocateNativeHostExe();
    }

    public string ExtensionFolderPath => LocateExtensionFolder();

    public ChromeNativeHostSetupStatus CheckStatus(ChromeNativeHostSetupRequest request)
    {
        ChromeNativeHostSetupStatus status = CreateBaseStatus(request);
        PopulateManifestStatus(status);
        PopulateRegistryStatus(status);
        PopulateMessages(status);
        return status;
    }

    public ChromeNativeHostSetupStatus Register(ChromeNativeHostSetupRequest request)
    {
        ChromeNativeHostSetupStatus status = CreateBaseStatus(request);
        if (!status.ExtensionOriginValid)
        {
            status.Errors.Add("Extension ID must be 32 lowercase Chrome ID characters, a through p.");
            PopulateManifestStatus(status);
            PopulateRegistryStatus(status);
            return status;
        }

        if (!status.NativeHostExeExists)
        {
            status.Errors.Add("MokoSnap.NativeHost.exe was not found. Build MokoSnap.NativeHost first.");
            PopulateManifestStatus(status);
            PopulateRegistryStatus(status);
            return status;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_manifestPath)!);
        ChromeNativeHostManifest manifest = ChromeNativeHostSetup.CreateManifest(
            _nativeHostExePath,
            request.ExtensionId);
        string json = JsonSerializer.Serialize(
            manifest,
            FileJsonStorage<ChromeNativeHostManifest>.CreateJsonSerializerOptions());
        File.WriteAllText(_manifestPath, json);
        _registry.SetRegisteredManifestPath(_manifestPath);

        status = CheckStatus(request);
        if (status.Ready)
        {
            status.Warnings.Insert(0, "Native host registered for current user. Restart Chrome before capturing tabs.");
        }

        return status;
    }

    private ChromeNativeHostSetupStatus CreateBaseStatus(ChromeNativeHostSetupRequest request)
    {
        string extensionId = request.ExtensionId.Trim();
        bool validExtensionId = ChromeNativeHostSetup.IsValidExtensionId(extensionId);
        return new ChromeNativeHostSetupStatus
        {
            ExtensionId = extensionId,
            ExtensionIdPresent = !string.IsNullOrWhiteSpace(extensionId),
            ExtensionOriginValid = validExtensionId,
            AllowedOrigin = validExtensionId ? ChromeNativeHostSetup.BuildAllowedOrigin(extensionId) : string.Empty,
            NativeHostExePath = _nativeHostExePath,
            NativeHostExeExists = File.Exists(_nativeHostExePath),
            ManifestPath = _manifestPath,
            ManifestFileExists = File.Exists(_manifestPath),
            LatestCapturePath = _latestCapturePath,
            LatestCaptureFileExists = File.Exists(_latestCapturePath)
        };
    }

    private void PopulateManifestStatus(ChromeNativeHostSetupStatus status)
    {
        if (!status.ManifestFileExists)
        {
            status.ManifestJsonValid = false;
            return;
        }

        try
        {
            string json = File.ReadAllText(_manifestPath);
            ChromeNativeHostManifest? manifest = JsonSerializer.Deserialize<ChromeNativeHostManifest>(
                json,
                FileJsonStorage<ChromeNativeHostManifest>.CreateJsonSerializerOptions());
            status.ManifestJsonValid =
                manifest is not null &&
                manifest.Name == ChromeNativeHostSetup.HostName &&
                manifest.Type == "stdio" &&
                !string.IsNullOrWhiteSpace(manifest.Path) &&
                manifest.AllowedOrigins.Count > 0;
        }
        catch (JsonException)
        {
            status.ManifestJsonValid = false;
        }
        catch (IOException)
        {
            status.ManifestJsonValid = false;
        }
    }

    private void PopulateRegistryStatus(ChromeNativeHostSetupStatus status)
    {
        status.RegistryKeyExists = _registry.KeyExists();
        status.RegisteredManifestPath = _registry.GetRegisteredManifestPath() ?? string.Empty;
        status.RegistryValuePointsToExpectedManifest = string.Equals(
            status.RegisteredManifestPath,
            _manifestPath,
            StringComparison.OrdinalIgnoreCase);
    }

    private static void PopulateMessages(ChromeNativeHostSetupStatus status)
    {
        if (!status.ExtensionIdPresent)
        {
            status.Warnings.Add("Paste the unpacked Chrome extension ID from chrome://extensions.");
        }
        else if (!status.ExtensionOriginValid)
        {
            status.Errors.Add("Extension ID must be 32 lowercase Chrome ID characters, a through p.");
        }

        if (!status.NativeHostExeExists)
        {
            status.Errors.Add("MokoSnap.NativeHost.exe was not found. Build MokoSnap.NativeHost first.");
        }

        if (!status.ManifestFileExists)
        {
            status.Warnings.Add("Native host manifest has not been generated yet.");
        }
        else if (!status.ManifestJsonValid)
        {
            status.Errors.Add("Native host manifest exists but is not valid.");
        }

        if (!status.RegistryKeyExists)
        {
            status.Warnings.Add("HKCU Native Messaging registry key is not registered.");
        }
        else if (!status.RegistryValuePointsToExpectedManifest)
        {
            status.Errors.Add("HKCU registry value does not point to the expected manifest path.");
        }

        if (!status.LatestCaptureFileExists)
        {
            status.Warnings.Add("No latest Chrome capture file exists yet.");
        }
    }

    private static string LocateNativeHostExe()
    {
        string baseDirectory = AppContext.BaseDirectory;
        List<string> candidates =
        [
            Path.Combine(baseDirectory, "MokoSnap.NativeHost.exe")
        ];

        DirectoryInfo? srcDirectory = FindAncestor(baseDirectory, "src");
        if (srcDirectory is not null)
        {
            string nativeHostBin = Path.Combine(srcDirectory.FullName, "MokoSnap.NativeHost", "bin");
            if (Directory.Exists(nativeHostBin))
            {
                candidates.AddRange(Directory.GetFiles(
                    nativeHostBin,
                    "MokoSnap.NativeHost.exe",
                    SearchOption.AllDirectories));
            }
        }

        return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
    }

    private static IChromeNativeHostRegistry CreateDefaultRegistry()
    {
#pragma warning disable CA1416
        return new WindowsChromeNativeHostRegistry();
#pragma warning restore CA1416
    }

    private static string LocateExtensionFolder()
    {
        DirectoryInfo? root = FindAncestor(AppContext.BaseDirectory, "MokoSnap");
        string candidate = root is null
            ? Path.Combine(AppContext.BaseDirectory, "extension", "chrome")
            : Path.Combine(root.FullName, "extension", "chrome");
        return candidate;
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
