using MokoSnap.Core.Models;

namespace MokoSnap.Core.ChromeCapture;

public static class ChromeTabCaptureImporter
{
    public static TargetConfig CreateChromeTarget(IEnumerable<ChromeTabInfo> tabs)
    {
        List<string> urls = tabs
            .Select(tab => tab.Url.Trim())
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .ToList();

        return new TargetConfig
        {
            Type = TargetType.Chrome,
            DisplayName = "Captured Chrome Tabs",
            OpenInNewWindow = true,
            Urls = urls
        };
    }
}
