using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MokoSnap.Core.Capture;

namespace MokoSnap.App.ViewModels;

public sealed class CapturedAppsDialogViewModel : INotifyPropertyChanged
{
    private readonly IVisibleAppCaptureService _captureService;
    private bool _includeExplorer;
    private string _statusMessage = string.Empty;

    public CapturedAppsDialogViewModel(IVisibleAppCaptureService captureService)
    {
        _captureService = captureService;
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<CapturedAppPreviewItemViewModel> Apps { get; } = [];

    public AsyncRelayCommand RefreshCommand { get; }

    public bool IncludeExplorer
    {
        get => _includeExplorer;
        set
        {
            if (_includeExplorer == value)
            {
                return;
            }

            _includeExplorer = value;
            OnPropertyChanged();
            RefreshCommand.Execute(null);
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<CapturedWindowApp> SelectedApps =>
        Apps
            .Where(app => app.IsSelected && app.IsEnabled)
            .Select(app => app.App)
            .ToList();

    private async Task RefreshAsync()
    {
        IReadOnlyList<CapturedWindowApp> capturedApps = await _captureService.CaptureVisibleAppsAsync(
            new VisibleAppCaptureOptions { IncludeExplorer = IncludeExplorer });

        Apps.Clear();
        foreach (CapturedWindowApp app in capturedApps)
        {
            Apps.Add(new CapturedAppPreviewItemViewModel(app));
        }

        int unavailableCount = Apps.Count(app => !app.IsEnabled);
        StatusMessage = unavailableCount == 0
            ? $"Found {Apps.Count} visible app(s)."
            : $"Found {Apps.Count} visible app(s); {unavailableCount} unavailable.";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
