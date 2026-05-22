using MokoSnap.Core.Running;

namespace MokoSnap.Platform.Windows.Closing;

public sealed class WindowsVisibleWindowCloser : IVisibleWindowCloser
{
    private readonly IVisibleWindowCloseCandidateProvider _candidateProvider;
    private readonly IGracefulWindowCloseBackend _closeBackend;
    private readonly ICloseWindowsConfirmationService? _confirmationService;
    private readonly TimeSpan _closeTimeout;
    private readonly TimeSpan _pollInterval;

    public WindowsVisibleWindowCloser(
        ICloseWindowsConfirmationService? confirmationService = null,
        IVisibleWindowCloseCandidateProvider? candidateProvider = null,
        IGracefulWindowCloseBackend? closeBackend = null,
        TimeSpan? closeTimeout = null,
        TimeSpan? pollInterval = null)
    {
        _confirmationService = confirmationService;
        _candidateProvider = candidateProvider ?? new WindowsVisibleWindowCloseCandidateProvider();
        _closeBackend = closeBackend ?? new WindowsGracefulWindowCloseBackend();
        _closeTimeout = closeTimeout ?? TimeSpan.FromSeconds(2);
        _pollInterval = pollInterval ?? TimeSpan.FromMilliseconds(50);
    }

    public async Task<CloseWindowsResult> CloseVisibleWindowsAsync(
        CloseWindowsRequest request,
        CancellationToken cancellationToken = default)
    {
        List<CloseWindowCandidate> candidates = (await _candidateProvider.GetCandidatesAsync(
            request.IncludeExplorer,
            cancellationToken)).ToList();

        CloseWindowsResult result = new()
        {
            CandidateWindows = candidates
        };

        if (candidates.Count == 0)
        {
            result.Message = "No visible windows to close.";
            return result;
        }

        List<CloseWindowCandidate> selectedCandidates = candidates;
        if (request.ConfirmBeforeClosing)
        {
            CloseWindowsSelectionResult selection = await ConfirmSelectionAsync(candidates, cancellationToken);
            if (selection.Canceled)
            {
                result.Succeeded = false;
                result.Canceled = true;
                result.SkippedWindows = candidates;
                result.Message = "Window closing canceled.";
                return result;
            }

            HashSet<long> selectedHandles = selection.SelectedWindowHandles.ToHashSet();
            selectedCandidates = candidates
                .Where(candidate => selectedHandles.Contains(candidate.WindowHandle))
                .ToList();
            result.SkippedWindows = candidates
                .Where(candidate => !selectedHandles.Contains(candidate.WindowHandle))
                .ToList();
        }

        foreach (CloseWindowCandidate candidate in selectedCandidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_closeBackend.RequestClose(candidate.WindowHandle))
            {
                result.FailedWindows.Add(new CloseWindowFailure
                {
                    Window = candidate,
                    Message = "Could not send close request."
                });
                continue;
            }

            if (await WaitForWindowToCloseAsync(candidate.WindowHandle, cancellationToken))
            {
                result.ClosedWindows.Add(candidate);
            }
            else
            {
                result.FailedWindows.Add(new CloseWindowFailure
                {
                    Window = candidate,
                    Message = "Window did not close before timeout."
                });
            }
        }

        result.ClosedWindowCount = result.ClosedWindows.Count;
        result.Succeeded = result.FailedWindows.Count == 0;
        result.Message = BuildMessage(result);
        return result;
    }

    private async Task<CloseWindowsSelectionResult> ConfirmSelectionAsync(
        IReadOnlyList<CloseWindowCandidate> candidates,
        CancellationToken cancellationToken)
    {
        if (_confirmationService is null)
        {
            return new CloseWindowsSelectionResult { Canceled = true };
        }

        return await _confirmationService.ConfirmCloseWindowsAsync(candidates, cancellationToken);
    }

    private async Task<bool> WaitForWindowToCloseAsync(long windowHandle, CancellationToken cancellationToken)
    {
        DateTimeOffset deadline = DateTimeOffset.UtcNow + _closeTimeout;
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (!_closeBackend.IsWindowOpen(windowHandle))
            {
                return true;
            }

            await _closeBackend.DelayAsync(_pollInterval, cancellationToken);
        }

        return !_closeBackend.IsWindowOpen(windowHandle);
    }

    private static string BuildMessage(CloseWindowsResult result)
    {
        return result.FailedWindows.Count == 0
            ? $"Closed {result.ClosedWindowCount} visible window(s)."
            : $"Closed {result.ClosedWindowCount} visible window(s); {result.FailedWindows.Count} failed.";
    }
}
