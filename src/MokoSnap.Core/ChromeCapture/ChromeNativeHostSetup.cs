using System.Text.Json.Serialization;

namespace MokoSnap.Core.ChromeCapture;

public static class ChromeNativeHostSetup
{
    public const string HostName = "com.mokosnap.chrome_capture";
    public const string RegistrySubKey = @"Software\Google\Chrome\NativeMessagingHosts\" + HostName;

    public static bool IsValidExtensionId(string extensionId)
    {
        string value = extensionId.Trim();
        return value.Length == 32 && value.All(character => character is >= 'a' and <= 'p');
    }

    public static string BuildAllowedOrigin(string extensionId)
    {
        return $"chrome-extension://{extensionId.Trim()}/";
    }

    public static ChromeNativeHostManifest CreateManifest(string nativeHostExePath, string extensionId)
    {
        return new ChromeNativeHostManifest
        {
            Name = HostName,
            Description = "MokoSnap Chrome tab capture native host",
            Path = nativeHostExePath,
            Type = "stdio",
            AllowedOrigins = [BuildAllowedOrigin(extensionId)]
        };
    }
}

public sealed class ChromeNativeHostSetupRequest
{
    public string ExtensionId { get; set; } = string.Empty;
}

public sealed class ChromeNativeHostSetupStatus
{
    public string ExtensionId { get; set; } = string.Empty;

    public bool ExtensionIdPresent { get; set; }

    public bool ExtensionOriginValid { get; set; }

    public string AllowedOrigin { get; set; } = string.Empty;

    public string NativeHostExePath { get; set; } = string.Empty;

    public bool NativeHostExeExists { get; set; }

    public string ManifestPath { get; set; } = string.Empty;

    public bool ManifestFileExists { get; set; }

    public bool ManifestJsonValid { get; set; }

    public bool RegistryKeyExists { get; set; }

    public string RegisteredManifestPath { get; set; } = string.Empty;

    public bool RegistryValuePointsToExpectedManifest { get; set; }

    public string LatestCapturePath { get; set; } = string.Empty;

    public bool LatestCaptureFileExists { get; set; }

    public List<string> Warnings { get; set; } = [];

    public List<string> Errors { get; set; } = [];

    public bool Ready =>
        ExtensionOriginValid &&
        NativeHostExeExists &&
        ManifestFileExists &&
        ManifestJsonValid &&
        RegistryKeyExists &&
        RegistryValuePointsToExpectedManifest;
}

public sealed class ChromeNativeHostManifest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "stdio";

    [JsonPropertyName("allowed_origins")]
    public List<string> AllowedOrigins { get; set; } = [];
}
