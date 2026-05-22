namespace MokoSnap.Platform.Windows.Closing;

public interface IGracefulWindowCloseBackend
{
    bool RequestClose(long windowHandle);

    bool IsWindowOpen(long windowHandle);

    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default);
}
