using MokoSnap.Core.Running;
using MokoSnap.Platform.Windows.Closing;

namespace MokoSnap.Tests;

public class WindowsVisibleWindowCloserTests
{
    [Fact]
    public async Task SkipConfirmationClosesAllCandidatesGracefully()
    {
        FakeCandidateProvider provider = new(
        [
            CreateCandidate(1, "One"),
            CreateCandidate(2, "Two")
        ]);
        FakeCloseBackend backend = new();
        backend.OpenWindows.Add(1);
        backend.OpenWindows.Add(2);
        WindowsVisibleWindowCloser closer = new(
            candidateProvider: provider,
            closeBackend: backend,
            closeTimeout: TimeSpan.FromMilliseconds(1),
            pollInterval: TimeSpan.FromMilliseconds(1));

        CloseWindowsResult result = await closer.CloseVisibleWindowsAsync(new CloseWindowsRequest());

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.ClosedWindowCount);
        Assert.Equal([1, 2], backend.CloseRequests);
        Assert.Empty(result.FailedWindows);
    }

    [Fact]
    public async Task ConfirmationCancelSkipsCloseRequests()
    {
        FakeCandidateProvider provider = new([CreateCandidate(1, "One")]);
        FakeCloseBackend backend = new();
        FakeConfirmationService confirmation = new()
        {
            Result = new CloseWindowsSelectionResult { Canceled = true }
        };
        WindowsVisibleWindowCloser closer = new(
            confirmation,
            provider,
            backend);

        CloseWindowsResult result = await closer.CloseVisibleWindowsAsync(new CloseWindowsRequest
        {
            ConfirmBeforeClosing = true
        });

        Assert.False(result.Succeeded);
        Assert.True(result.Canceled);
        Assert.Empty(backend.CloseRequests);
        Assert.Single(result.SkippedWindows);
    }

    [Fact]
    public async Task ConfirmationSelectionSkipsUncheckedCandidates()
    {
        FakeCandidateProvider provider = new(
        [
            CreateCandidate(1, "One"),
            CreateCandidate(2, "Two")
        ]);
        FakeCloseBackend backend = new();
        backend.OpenWindows.Add(1);
        FakeConfirmationService confirmation = new()
        {
            Result = new CloseWindowsSelectionResult
            {
                SelectedWindowHandles = [1]
            }
        };
        WindowsVisibleWindowCloser closer = new(
            confirmation,
            provider,
            backend);

        CloseWindowsResult result = await closer.CloseVisibleWindowsAsync(new CloseWindowsRequest
        {
            ConfirmBeforeClosing = true
        });

        Assert.True(result.Succeeded);
        Assert.Equal([1], backend.CloseRequests);
        CloseWindowCandidate skipped = Assert.Single(result.SkippedWindows);
        Assert.Equal(2, skipped.WindowHandle);
    }

    [Fact]
    public async Task TimeoutReportsFailedWindowWithoutForceKilling()
    {
        FakeCandidateProvider provider = new([CreateCandidate(1, "One")]);
        FakeCloseBackend backend = new() { KeepWindowsOpen = true };
        backend.OpenWindows.Add(1);
        WindowsVisibleWindowCloser closer = new(
            candidateProvider: provider,
            closeBackend: backend,
            closeTimeout: TimeSpan.FromMilliseconds(1),
            pollInterval: TimeSpan.FromMilliseconds(1));

        CloseWindowsResult result = await closer.CloseVisibleWindowsAsync(new CloseWindowsRequest());

        Assert.False(result.Succeeded);
        Assert.Empty(result.ClosedWindows);
        CloseWindowFailure failure = Assert.Single(result.FailedWindows);
        Assert.Equal(1, failure.Window.WindowHandle);
        Assert.Equal([1], backend.CloseRequests);
    }

    private static CloseWindowCandidate CreateCandidate(long handle, string title)
    {
        return new CloseWindowCandidate
        {
            WindowHandle = handle,
            WindowTitle = title,
            ProcessName = "app",
            ExecutablePath = @"C:\Tools\app.exe"
        };
    }

    private sealed class FakeCandidateProvider : IVisibleWindowCloseCandidateProvider
    {
        private readonly IReadOnlyList<CloseWindowCandidate> _candidates;

        public FakeCandidateProvider(IReadOnlyList<CloseWindowCandidate> candidates)
        {
            _candidates = candidates;
        }

        public Task<IReadOnlyList<CloseWindowCandidate>> GetCandidatesAsync(
            bool includeExplorer,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_candidates);
        }
    }

    private sealed class FakeCloseBackend : IGracefulWindowCloseBackend
    {
        public List<long> CloseRequests { get; } = [];

        public HashSet<long> OpenWindows { get; } = [];

        public bool KeepWindowsOpen { get; set; }

        public bool RequestClose(long windowHandle)
        {
            CloseRequests.Add(windowHandle);
            if (!KeepWindowsOpen)
            {
                OpenWindows.Remove(windowHandle);
            }

            return true;
        }

        public bool IsWindowOpen(long windowHandle)
        {
            return OpenWindows.Contains(windowHandle);
        }

        public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeConfirmationService : ICloseWindowsConfirmationService
    {
        public CloseWindowsSelectionResult Result { get; set; } = new();

        public Task<CloseWindowsSelectionResult> ConfirmCloseWindowsAsync(
            IReadOnlyList<CloseWindowCandidate> candidates,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result);
        }
    }
}
