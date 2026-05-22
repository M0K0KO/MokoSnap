namespace MokoSnap.Core.ChromeCapture;

public sealed class ChromeTabCapture
{
    public DateTimeOffset CapturedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<ChromeWindowCapture> Windows { get; set; } = [];

    public List<ChromeTabInfo> Tabs { get; set; } = [];
}

public sealed class ChromeWindowCapture
{
    public int WindowId { get; set; }

    public List<ChromeTabInfo> Tabs { get; set; } = [];
}

public sealed class ChromeTabInfo
{
    public int WindowId { get; set; }

    public int TabId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public bool Active { get; set; }

    public bool Pinned { get; set; }

    public int Index { get; set; }
}
