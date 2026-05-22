using System.Windows;
using MokoSnap.App.Views;
using MokoSnap.Core.Running;

namespace MokoSnap.App.Services;

public sealed class CloseWindowsConfirmationService : ICloseWindowsConfirmationService
{
    public Task<CloseWindowsSelectionResult> ConfirmCloseWindowsAsync(
        IReadOnlyList<CloseWindowCandidate> candidates,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        CloseWindowsDialog dialog = new(candidates)
        {
            Owner = Application.Current.MainWindow
        };

        bool? result = dialog.ShowDialog();
        return Task.FromResult(result == true
            ? new CloseWindowsSelectionResult
            {
                SelectedWindowHandles = dialog.SelectedWindowHandles.ToList()
            }
            : new CloseWindowsSelectionResult { Canceled = true });
    }
}
