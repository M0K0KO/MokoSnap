using System.Windows;
using System.Windows.Input;
using MokoSnap.App.Services;
using MokoSnap.App.ViewModels;
using MokoSnap.Core.Running;

namespace MokoSnap.App.Views;

public partial class CloseWindowsDialog : Window
{
    private readonly CloseWindowsDialogViewModel _viewModel;

    public CloseWindowsDialog(IReadOnlyList<CloseWindowCandidate> candidates)
    {
        InitializeComponent();
        _viewModel = new CloseWindowsDialogViewModel(candidates);
        DataContext = _viewModel;
        Loaded += OnLoaded;
        PreviewKeyDown += OnPreviewKeyDown;
    }

    public IReadOnlyList<long> SelectedWindowHandles => _viewModel.SelectedWindowHandles;

    private void OnCloseSelectedClicked(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CandidateGrid.SelectedIndex = _viewModel.Windows.Count > 0 ? 0 : -1;
        DialogFocusHelper.ActivateAndFocus(this, CandidateGrid);
    }

    private void OnCandidateGridPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Space ||
            CandidateGrid.SelectedItem is not CloseWindowCandidateItemViewModel selectedWindow)
        {
            return;
        }

        selectedWindow.IsSelected = !selectedWindow.IsSelected;
        e.Handled = true;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            DialogResult = true;
            e.Handled = true;
        }
    }
}
