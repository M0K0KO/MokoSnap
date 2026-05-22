using MokoSnap.Core.Running;

namespace MokoSnap.App.Services;

public sealed class NotImplementedVisibleWindowCloser : IVisibleWindowCloser
{
    public Task<CloseWindowsResult> CloseVisibleWindowsAsync(
        CloseWindowsRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CloseWindowsResult
        {
            Succeeded = false,
            Message = "Close visible windows is not implemented yet."
        });
    }
}
