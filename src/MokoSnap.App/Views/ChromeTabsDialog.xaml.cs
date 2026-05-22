using System.Windows;
using MokoSnap.App.Services;
using MokoSnap.App.ViewModels;
using MokoSnap.Core.ChromeCapture;

namespace MokoSnap.App.Views;

public partial class ChromeTabsDialog : Window
{
    private readonly ChromeTabsDialogViewModel _viewModel;

    public ChromeTabsDialog(ChromeTabCapture capture)
    {
        InitializeComponent();
        _viewModel = new ChromeTabsDialogViewModel(capture);
        DataContext = _viewModel;
        Loaded += OnLoaded;
    }

    public IReadOnlyList<ChromeTabInfo> SelectedTabs => _viewModel.SelectedTabs;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        DialogFocusHelper.ActivateAndFocus(this, this);
    }

    private void OnSelectAllClicked(object sender, RoutedEventArgs e)
    {
        _viewModel.SelectAll(true);
    }

    private void OnDeselectAllClicked(object sender, RoutedEventArgs e)
    {
        _viewModel.SelectAll(false);
    }

    private void OnImportSelectedClicked(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
