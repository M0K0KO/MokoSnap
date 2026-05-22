using System.Windows;
using MokoSnap.App.Services;
using MokoSnap.App.ViewModels;
using MokoSnap.Core.Storage;

namespace MokoSnap.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(
            MokoSnapStoragePaths.CreateAppSettingsStorage(),
            new MessageBoxConfirmationService());
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
