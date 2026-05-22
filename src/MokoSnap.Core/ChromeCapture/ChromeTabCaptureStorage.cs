using System.Text.Json;
using MokoSnap.Core.Storage;

namespace MokoSnap.Core.ChromeCapture;

public sealed class ChromeTabCaptureStorage
{
    public ChromeTabCaptureStorage(string filePath)
    {
        FilePath = filePath;
    }

    public string FilePath { get; }

    public async Task<ChromeTabCaptureLoadResult> LoadLatestAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(FilePath))
        {
            return ChromeTabCaptureLoadResult.Missing(FilePath);
        }

        try
        {
            await using FileStream stream = File.OpenRead(FilePath);
            ChromeTabCapture? capture = await JsonSerializer.DeserializeAsync<ChromeTabCapture>(
                stream,
                FileJsonStorage<ChromeTabCapture>.CreateJsonSerializerOptions(),
                cancellationToken);

            return capture is null
                ? ChromeTabCaptureLoadResult.Invalid("Chrome capture file is empty or invalid.")
                : ChromeTabCaptureLoadResult.Success(capture);
        }
        catch (JsonException ex)
        {
            return ChromeTabCaptureLoadResult.Invalid($"Chrome capture file is invalid: {ex.Message}");
        }
        catch (IOException ex)
        {
            return ChromeTabCaptureLoadResult.Invalid($"Chrome capture file could not be read: {ex.Message}");
        }
    }
}

public sealed class ChromeTabCaptureLoadResult
{
    private ChromeTabCaptureLoadResult(
        ChromeTabCapture? capture,
        ChromeTabCaptureLoadStatus status,
        string message)
    {
        Capture = capture;
        Status = status;
        Message = message;
    }

    public ChromeTabCapture? Capture { get; }

    public ChromeTabCaptureLoadStatus Status { get; }

    public string Message { get; }

    public bool Succeeded => Status == ChromeTabCaptureLoadStatus.Success;

    public static ChromeTabCaptureLoadResult Success(ChromeTabCapture capture)
    {
        return new ChromeTabCaptureLoadResult(capture, ChromeTabCaptureLoadStatus.Success, string.Empty);
    }

    public static ChromeTabCaptureLoadResult Missing(string path)
    {
        return new ChromeTabCaptureLoadResult(
            null,
            ChromeTabCaptureLoadStatus.Missing,
            $"No Chrome tab capture was found. Use the MokoSnap Chrome extension to capture tabs first. Expected file: {path}");
    }

    public static ChromeTabCaptureLoadResult Invalid(string message)
    {
        return new ChromeTabCaptureLoadResult(null, ChromeTabCaptureLoadStatus.Invalid, message);
    }
}

public enum ChromeTabCaptureLoadStatus
{
    Success,
    Missing,
    Invalid
}
