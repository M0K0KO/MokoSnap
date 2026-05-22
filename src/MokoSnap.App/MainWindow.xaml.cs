using System.Windows;
using MokoSnap.App.Services;
using MokoSnap.App.ViewModels;
using MokoSnap.Core.Running;
using MokoSnap.Core.Storage;
using MokoSnap.Platform.Windows.Capture;
using MokoSnap.Platform.Windows.Launching;

namespace MokoSnap.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(
            MokoSnapStoragePaths.CreateAppSettingsStorage(),
            new MessageBoxConfirmationService(),
            new CapturedAppSelectionService(new WindowsVisibleAppCaptureService()),
            new PresetRunnerService(
                new WindowsTargetLauncher(),
                new NotImplementedVisibleWindowCloser(),
                new SystemLaunchDelay()));
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}
