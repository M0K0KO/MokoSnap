using System.Collections.ObjectModel;
using MokoSnap.Core.ChromeCapture;

namespace MokoSnap.App.ViewModels;

public sealed class ChromeTabsDialogViewModel
{
    public ChromeTabsDialogViewModel(ChromeTabCapture capture)
    {
        CapturedAt = capture.CapturedAt.ToLocalTime().ToString("g");
        IReadOnlyList<ChromeWindowCapture> windows = capture.Windows.Count > 0
            ? capture.Windows
            : BuildWindows(capture.Tabs);

        foreach (ChromeWindowCapture window in windows)
        {
            Windows.Add(new ChromeWindowCaptureViewModel(window));
        }
    }

    public string CapturedAt { get; }

    public ObservableCollection<ChromeWindowCaptureViewModel> Windows { get; } = [];

    public IReadOnlyList<ChromeTabInfo> SelectedTabs =>
        Windows
            .SelectMany(window => window.Tabs)
            .Where(tab => tab.IsSelected)
            .Select(tab => tab.Tab)
            .ToList();

    public void SelectAll(bool selected)
    {
        foreach (ChromeTabItemViewModel tab in Windows.SelectMany(window => window.Tabs))
        {
            tab.IsSelected = selected;
        }
    }

    private static IReadOnlyList<ChromeWindowCapture> BuildWindows(IReadOnlyList<ChromeTabInfo> tabs)
    {
        return tabs
            .GroupBy(tab => tab.WindowId)
            .Select(group => new ChromeWindowCapture
            {
                WindowId = group.Key,
                Tabs = group.OrderBy(tab => tab.Index).ToList()
            })
            .ToList();
    }
}
