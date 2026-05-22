using System.Collections.ObjectModel;
using MokoSnap.Core.ChromeCapture;

namespace MokoSnap.App.ViewModels;

public sealed class ChromeWindowCaptureViewModel
{
    public ChromeWindowCaptureViewModel(ChromeWindowCapture window)
    {
        WindowId = window.WindowId;
        foreach (ChromeTabInfo tab in window.Tabs.OrderBy(tab => tab.Index))
        {
            Tabs.Add(new ChromeTabItemViewModel(tab));
        }
    }

    public int WindowId { get; }

    public string Header => $"Chrome Window {WindowId}";

    public ObservableCollection<ChromeTabItemViewModel> Tabs { get; } = [];
}
