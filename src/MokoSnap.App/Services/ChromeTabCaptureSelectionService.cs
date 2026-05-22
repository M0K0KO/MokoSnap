using MokoSnap.App.Views;
using MokoSnap.Core.ChromeCapture;

namespace MokoSnap.App.Services;

public sealed class ChromeTabCaptureSelectionService : IChromeTabCaptureSelectionService
{
    private readonly ChromeTabCaptureStorage _storage;

    public ChromeTabCaptureSelectionService(ChromeTabCaptureStorage storage)
    {
        _storage = storage;
    }

    public async Task<ChromeTabCaptureSelectionResult> SelectLatestCaptureAsync(
        CancellationToken cancellationToken = default)
    {
        ChromeTabCaptureLoadResult loadResult = await _storage.LoadLatestAsync(cancellationToken);
        if (!loadResult.Succeeded || loadResult.Capture is null)
        {
            return new ChromeTabCaptureSelectionResult { Message = loadResult.Message };
        }

        ChromeTabsDialog dialog = new(loadResult.Capture)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        bool? result = dialog.ShowDialog();
        if (result != true)
        {
            return new ChromeTabCaptureSelectionResult { Message = "No Chrome tabs imported." };
        }

        return new ChromeTabCaptureSelectionResult
        {
            SelectedTabs = dialog.SelectedTabs,
            Message = dialog.SelectedTabs.Count == 0 ? "No Chrome tabs selected." : string.Empty
        };
    }
}
