using System.ComponentModel;
using System.Runtime.CompilerServices;
using MokoSnap.Core.Capture;

namespace MokoSnap.App.ViewModels;

public sealed class CapturedAppPreviewItemViewModel : INotifyPropertyChanged
{
    private bool _isSelected;

    public CapturedAppPreviewItemViewModel(CapturedWindowApp app)
    {
        App = app;
        IsSelected = app.IsAvailable;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public CapturedWindowApp App { get; }

    public bool IsEnabled => App.IsAvailable;

    public string Title => App.WindowTitle;

    public string ProcessName => App.ProcessName;

    public string ExecutablePath => App.IsAvailable ? App.ExecutablePath : "Executable path unavailable";

    public bool IsPlacementAvailable => App.WindowPlacement is not null;

    public string PlacementSummary => App.WindowPlacement is null
        ? "Not captured"
        : $"{App.WindowPlacement.ShowState} {App.WindowPlacement.Left},{App.WindowPlacement.Top} {App.WindowPlacement.Width}x{App.WindowPlacement.Height}";

    public bool RememberWindowPlacement
    {
        get => App.WindowPlacement?.Enabled == true;
        set
        {
            if (App.WindowPlacement is null || App.WindowPlacement.Enabled == value)
            {
                return;
            }

            App.WindowPlacement.Enabled = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RememberWindowPlacement)));
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value && IsEnabled;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }
}
