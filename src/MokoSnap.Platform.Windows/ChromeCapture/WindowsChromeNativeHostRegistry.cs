using Microsoft.Win32;
using MokoSnap.Core.ChromeCapture;
using System.Runtime.Versioning;

namespace MokoSnap.Platform.Windows.ChromeCapture;

[SupportedOSPlatform("windows")]
public sealed class WindowsChromeNativeHostRegistry : IChromeNativeHostRegistry
{
    public bool KeyExists()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(ChromeNativeHostSetup.RegistrySubKey);
        return key is not null;
    }

    public string? GetRegisteredManifestPath()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(ChromeNativeHostSetup.RegistrySubKey);
        return key?.GetValue(null) as string;
    }

    public void SetRegisteredManifestPath(string manifestPath)
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(ChromeNativeHostSetup.RegistrySubKey);
        key.SetValue(null, manifestPath, RegistryValueKind.String);
    }
}
