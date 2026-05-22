using System.Text.Json;
using System.Text.Json.Serialization;
using MokoSnap.Core.ChromeCapture;
using MokoSnap.Core.NativeMessaging;
using MokoSnap.Core.Storage;

try
{
    Stream input = Console.OpenStandardInput();
    Stream output = Console.OpenStandardOutput();

    ChromeCaptureNativeMessage? message = await NativeMessageProtocol.ReadMessageAsync<ChromeCaptureNativeMessage>(input);
    if (message is null)
    {
        return;
    }

    NativeHostResponse response = await HandleMessageAsync(message);
    await NativeMessageProtocol.WriteMessageAsync(output, response);
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    await NativeMessageProtocol.WriteMessageAsync(
        Console.OpenStandardOutput(),
        NativeHostResponse.Failed(ex.Message));
}

static async Task<NativeHostResponse> HandleMessageAsync(ChromeCaptureNativeMessage message)
{
    if (!message.Type.Equals("captureTabs", StringComparison.OrdinalIgnoreCase))
    {
        return NativeHostResponse.Failed($"Unsupported message type '{message.Type}'.");
    }

    ChromeTabCapture capture = message.ToCapture();
    Directory.CreateDirectory(MokoSnapStoragePaths.AppDataDirectory);
    await using FileStream stream = File.Create(MokoSnapStoragePaths.ChromeTabsLatestPath);
    await JsonSerializer.SerializeAsync(
        stream,
        capture,
        FileJsonStorage<ChromeTabCapture>.CreateJsonSerializerOptions());

    return NativeHostResponse.Succeeded(MokoSnapStoragePaths.ChromeTabsLatestPath);
}

public sealed class ChromeCaptureNativeMessage
{
    public string Type { get; set; } = string.Empty;

    public DateTimeOffset CapturedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<ChromeWindowCapture> Windows { get; set; } = [];

    public List<ChromeTabInfo> Tabs { get; set; } = [];

    public ChromeTabCapture ToCapture()
    {
        return new ChromeTabCapture
        {
            CapturedAt = CapturedAt,
            Windows = Windows,
            Tabs = Tabs.Count > 0 ? Tabs : Windows.SelectMany(window => window.Tabs).ToList()
        };
    }
}

public sealed class NativeHostResponse
{
    public string Type { get; init; } = "captureTabsResult";

    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public string FilePath { get; init; } = string.Empty;

    public static NativeHostResponse Succeeded(string filePath)
    {
        return new NativeHostResponse
        {
            Success = true,
            Message = "Chrome tabs captured.",
            FilePath = filePath
        };
    }

    public static NativeHostResponse Failed(string message)
    {
        return new NativeHostResponse
        {
            Success = false,
            Message = message
        };
    }
}
