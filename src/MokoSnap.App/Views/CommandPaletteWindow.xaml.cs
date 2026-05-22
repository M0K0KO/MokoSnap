using System.Windows;
using System.Windows.Input;
using MokoSnap.App.Services;
using MokoSnap.App.ViewModels;

namespace MokoSnap.App.Views;

public partial class CommandPaletteWindow : Window
{
    public CommandPaletteWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        PreviewKeyDown += OnPreviewKeyDown;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        DialogFocusHelper.ActivateAndFocus(this, SearchBox);
    }

    private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (DataContext is not CommandPaletteViewModel viewModel)
        {
            return;
        }

        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && viewModel.SelectedItem is not null)
        {
            DialogResult = true;
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            viewModel.SelectNext();
            PresetList.ScrollIntoView(viewModel.SelectedItem);
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            viewModel.SelectPrevious();
            PresetList.ScrollIntoView(viewModel.SelectedItem);
            e.Handled = true;
        }
    }
}
