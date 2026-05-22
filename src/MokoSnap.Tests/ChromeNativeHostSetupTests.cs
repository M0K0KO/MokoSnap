using System.Text.Json;
using MokoSnap.Core.ChromeCapture;
using MokoSnap.Core.Storage;
using MokoSnap.Platform.Windows.ChromeCapture;

namespace MokoSnap.Tests;

public class ChromeNativeHostSetupTests
{
    [Fact]
    public void ValidatesChromeExtensionIdFormat()
    {
        Assert.True(ChromeNativeHostSetup.IsValidExtensionId("abcdefghijklmnopabcdefghijklmnop"));
        Assert.False(ChromeNativeHostSetup.IsValidExtensionId("abcdefghijklmnopabcdefghijklmnox"));
        Assert.False(ChromeNativeHostSetup.IsValidExtensionId("short"));
        Assert.False(ChromeNativeHostSetup.IsValidExtensionId("ABCDEFGHIJKLMNOPABCDEFGHIJKLMNOP"));
    }

    [Fact]
    public void BuildsAllowedOrigin()
    {
        string origin = ChromeNativeHostSetup.BuildAllowedOrigin("abcdefghijklmnopabcdefghijklmnop");

        Assert.Equal("chrome-extension://abcdefghijklmnopabcdefghijklmnop/", origin);
    }

    [Fact]
    public void ManifestJsonUsesNativeMessagingSchemaNames()
    {
        ChromeNativeHostManifest manifest = ChromeNativeHostSetup.CreateManifest(
            @"C:\Tools\MokoSnap.NativeHost.exe",
            "abcdefghijklmnopabcdefghijklmnop");

        string json = JsonSerializer.Serialize(
            manifest,
            FileJsonStorage<ChromeNativeHostManifest>.CreateJsonSerializerOptions());

        Assert.Contains("\"allowed_origins\"", json);
        Assert.Contains("chrome-extension://abcdefghijklmnopabcdefghijklmnop/", json);
        Assert.Contains(@"C:\\Tools\\MokoSnap.NativeHost.exe", json);
    }

    [Fact]
    public void NativeHostPathResolverPrefersExplicitPath()
    {
        string resolved = ChromeNativeHostPathResolver.Resolve(
            @"C:\App",
            @"C:\Configured\MokoSnap.NativeHost.exe",
            _ => false);

        Assert.Equal(@"C:\Configured\MokoSnap.NativeHost.exe", resolved);
    }

    [Fact]
    public void NativeHostPathResolverFindsPublishedSiblingNativeHost()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string appBaseDirectory = Path.Combine(directory, "artifacts", "publish", "MokoSnap", "MokoSnap.App");
        string nativeHostPath = Path.Combine(
            directory,
            "artifacts",
            "publish",
            "MokoSnap",
            "MokoSnap.NativeHost",
            "MokoSnap.NativeHost.exe");
        Directory.CreateDirectory(appBaseDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(nativeHostPath)!);
        File.WriteAllText(nativeHostPath, string.Empty);

        string resolved = ChromeNativeHostPathResolver.Resolve(appBaseDirectory, null, File.Exists);

        Assert.Equal(nativeHostPath, resolved);
    }

    [Fact]
    public void NativeHostPathResolverFindsInstalledSiblingNativeHost()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string appBaseDirectory = Path.Combine(directory, "Programs", "MokoSnap", "MokoSnap.App");
        string nativeHostPath = Path.Combine(
            directory,
            "Programs",
            "MokoSnap",
            "MokoSnap.NativeHost",
            "MokoSnap.NativeHost.exe");
        Directory.CreateDirectory(appBaseDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(nativeHostPath)!);
        File.WriteAllText(nativeHostPath, string.Empty);

        string resolved = ChromeNativeHostPathResolver.Resolve(appBaseDirectory, null, File.Exists);

        Assert.Equal(nativeHostPath, resolved);
    }

    [Fact]
    public void NativeHostPathResolverFindsDevelopmentReleaseBeforeDebug()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string appBaseDirectory = Path.Combine(directory, "src", "MokoSnap.App", "bin", "Debug", "net8.0-windows");
        string releaseNativeHostPath = Path.Combine(
            directory,
            "src",
            "MokoSnap.NativeHost",
            "bin",
            "Release",
            "net8.0",
            "MokoSnap.NativeHost.exe");
        string debugNativeHostPath = Path.Combine(
            directory,
            "src",
            "MokoSnap.NativeHost",
            "bin",
            "Debug",
            "net8.0",
            "MokoSnap.NativeHost.exe");
        Directory.CreateDirectory(appBaseDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(releaseNativeHostPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(debugNativeHostPath)!);
        File.WriteAllText(releaseNativeHostPath, string.Empty);
        File.WriteAllText(debugNativeHostPath, string.Empty);

        string resolved = ChromeNativeHostPathResolver.Resolve(appBaseDirectory, null, File.Exists);

        Assert.Equal(releaseNativeHostPath, resolved);
    }

    [Fact]
    public void RegisterWritesManifestAndHkcuRegistryValueThroughAbstraction()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string nativeHostPath = Path.Combine(directory, "MokoSnap.NativeHost.exe");
        string manifestPath = Path.Combine(directory, "chrome-native-host-manifest.json");
        string capturePath = Path.Combine(directory, "chrome-tabs-latest.json");
        File.WriteAllText(nativeHostPath, string.Empty);
        FakeChromeNativeHostRegistry registry = new();
        ChromeNativeHostSetupService service = new(
            registry,
            manifestPath,
            capturePath,
            nativeHostPath);

        ChromeNativeHostSetupStatus status = service.Register(new ChromeNativeHostSetupRequest
        {
            ExtensionId = "abcdefghijklmnopabcdefghijklmnop"
        });

        Assert.True(status.Ready);
        Assert.True(File.Exists(manifestPath));
        Assert.Equal(manifestPath, registry.RegisteredManifestPath);
        Assert.True(registry.KeyExists());
    }

    [Fact]
    public void RegisterWritesManifestWithPublishedNativeHostPath()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string appBaseDirectory = Path.Combine(directory, "artifacts", "publish", "MokoSnap", "MokoSnap.App");
        string nativeHostPath = Path.Combine(
            directory,
            "artifacts",
            "publish",
            "MokoSnap",
            "MokoSnap.NativeHost",
            "MokoSnap.NativeHost.exe");
        string manifestPath = Path.Combine(directory, "chrome-native-host-manifest.json");
        Directory.CreateDirectory(appBaseDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(nativeHostPath)!);
        File.WriteAllText(nativeHostPath, string.Empty);
        FakeChromeNativeHostRegistry registry = new();
        ChromeNativeHostSetupService service = new(
            registry,
            manifestPath,
            Path.Combine(directory, "chrome-tabs-latest.json"),
            appBaseDirectory: appBaseDirectory);

        ChromeNativeHostSetupStatus status = service.Register(new ChromeNativeHostSetupRequest
        {
            ExtensionId = "abcdefghijklmnopabcdefghijklmnop"
        });

        string json = File.ReadAllText(manifestPath);
        ChromeNativeHostManifest manifest = JsonSerializer.Deserialize<ChromeNativeHostManifest>(
            json,
            FileJsonStorage<ChromeNativeHostManifest>.CreateJsonSerializerOptions())!;
        Assert.True(status.Ready);
        Assert.Equal(nativeHostPath, manifest.Path);
    }

    [Fact]
    public void InvalidExtensionIdDoesNotWriteManifestOrRegistry()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string nativeHostPath = Path.Combine(directory, "MokoSnap.NativeHost.exe");
        string manifestPath = Path.Combine(directory, "chrome-native-host-manifest.json");
        File.WriteAllText(nativeHostPath, string.Empty);
        FakeChromeNativeHostRegistry registry = new();
        ChromeNativeHostSetupService service = new(
            registry,
            manifestPath,
            Path.Combine(directory, "chrome-tabs-latest.json"),
            nativeHostPath);

        ChromeNativeHostSetupStatus status = service.Register(new ChromeNativeHostSetupRequest
        {
            ExtensionId = "invalid"
        });

        Assert.False(status.Ready);
        Assert.False(File.Exists(manifestPath));
        Assert.False(registry.KeyExists());
        Assert.Contains(status.Errors, error => error.Contains("Extension ID", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class FakeChromeNativeHostRegistry : IChromeNativeHostRegistry
    {
        public string? RegisteredManifestPath { get; private set; }

        public bool KeyExists()
        {
            return RegisteredManifestPath is not null;
        }

        public string? GetRegisteredManifestPath()
        {
            return RegisteredManifestPath;
        }

        public void SetRegisteredManifestPath(string manifestPath)
        {
            RegisteredManifestPath = manifestPath;
        }
    }
}
