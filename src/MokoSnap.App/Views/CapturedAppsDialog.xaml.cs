using System.Windows;
using MokoSnap.App.ViewModels;
using MokoSnap.Core.Capture;

namespace MokoSnap.App.Views;

public partial class CapturedAppsDialog : Window
{
    private readonly CapturedAppsDialogViewModel _viewModel;

    public CapturedAppsDialog(IVisibleAppCaptureService captureService)
    {
        InitializeComponent();
        _viewModel = new CapturedAppsDialogViewModel(captureService);
        DataContext = _viewModel;
        Loaded += OnLoaded;
    }

    public IReadOnlyList<CapturedWindowApp> SelectedApps => _viewModel.SelectedApps;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.RefreshCommand.Execute(null);
    }

    private void OnAddSelectedClicked(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
