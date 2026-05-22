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
