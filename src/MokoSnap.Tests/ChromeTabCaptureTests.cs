using System.Text;
using System.Text.Json;
using MokoSnap.Core.ChromeCapture;
using MokoSnap.Core.Models;
using MokoSnap.Core.NativeMessaging;
using MokoSnap.Core.Storage;

namespace MokoSnap.Tests;

public class ChromeTabCaptureTests
{
    [Fact]
    public async Task NativeMessageProtocolRoundTripsLengthPrefixedMessage()
    {
        ChromeTabCapture capture = CreateCapture();
        await using MemoryStream stream = new();

        await NativeMessageProtocol.WriteMessageAsync(stream, capture);
        stream.Position = 0;
        ChromeTabCapture? roundTripped = await NativeMessageProtocol.ReadMessageAsync<ChromeTabCapture>(stream);

        Assert.NotNull(roundTripped);
        Assert.Single(roundTripped.Windows);
        Assert.Equal("https://example.com/one", roundTripped.Tabs[0].Url);
    }

    [Fact]
    public void ChromeTabCaptureJsonDeserializes()
    {
        string json = """
        {
          "capturedAt": "2026-05-23T00:00:00+00:00",
          "windows": [
            {
              "windowId": 10,
              "tabs": [
                {
                  "windowId": 10,
                  "tabId": 20,
                  "title": "Example",
                  "url": "https://example.com",
                  "active": true,
                  "pinned": false,
                  "index": 0
                }
              ]
            }
          ],
          "tabs": [
            {
              "windowId": 10,
              "tabId": 20,
              "title": "Example",
              "url": "https://example.com",
              "active": true,
              "pinned": false,
              "index": 0
            }
          ]
        }
        """;

        ChromeTabCapture? capture = JsonSerializer.Deserialize<ChromeTabCapture>(
            json,
            FileJsonStorage<ChromeTabCapture>.CreateJsonSerializerOptions());

        Assert.NotNull(capture);
        ChromeTabInfo tab = Assert.Single(capture.Tabs);
        Assert.Equal(10, tab.WindowId);
        Assert.Equal("Example", tab.Title);
        Assert.Equal("https://example.com", tab.Url);
        Assert.True(tab.Active);
    }

    [Fact]
    public void SelectedTabsConvertToChromeTargetInSelectionOrder()
    {
        ChromeTabInfo second = new() { Url = "https://example.com/two", Index = 1 };
        ChromeTabInfo first = new() { Url = "https://example.com/one", Index = 0 };

        TargetConfig target = ChromeTabCaptureImporter.CreateChromeTarget([second, first]);

        Assert.Equal(TargetType.Chrome, target.Type);
        Assert.True(target.OpenInNewWindow);
        Assert.Equal(["https://example.com/two", "https://example.com/one"], target.Urls);
    }

    [Fact]
    public async Task MissingCaptureFileReturnsClearResult()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "chrome-tabs-latest.json");
        ChromeTabCaptureStorage storage = new(path);

        ChromeTabCaptureLoadResult result = await storage.LoadLatestAsync();

        Assert.Equal(ChromeTabCaptureLoadStatus.Missing, result.Status);
        Assert.Contains("Chrome extension", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(path, result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvalidCaptureFileReturnsClearResult()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "chrome-tabs-latest.json");
        await File.WriteAllTextAsync(path, "{ invalid json", Encoding.UTF8);
        ChromeTabCaptureStorage storage = new(path);

        ChromeTabCaptureLoadResult result = await storage.LoadLatestAsync();

        Assert.Equal(ChromeTabCaptureLoadStatus.Invalid, result.Status);
        Assert.Contains("invalid JSON", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Capture tabs again", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(path, result.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static ChromeTabCapture CreateCapture()
    {
        ChromeTabInfo tab = new()
        {
            WindowId = 1,
            TabId = 2,
            Title = "One",
            Url = "https://example.com/one",
            Index = 0
        };

        return new ChromeTabCapture
        {
            CapturedAt = new DateTimeOffset(2026, 5, 23, 0, 0, 0, TimeSpan.Zero),
            Windows =
            [
                new ChromeWindowCapture
                {
                    WindowId = 1,
                    Tabs = [tab]
                }
            ],
            Tabs = [tab]
        };
    }
}
