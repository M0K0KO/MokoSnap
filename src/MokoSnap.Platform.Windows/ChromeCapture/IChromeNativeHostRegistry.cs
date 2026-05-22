namespace MokoSnap.Platform.Windows.ChromeCapture;

public interface IChromeNativeHostRegistry
{
    bool KeyExists();

    string? GetRegisteredManifestPath();

    void SetRegisteredManifestPath(string manifestPath);
}
