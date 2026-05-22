using System.Windows;
using System.Windows.Input;
using MokoSnap.App.Services;
using MokoSnap.App.ViewModels;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace MokoSnap.App.Views;

public partial class SettingsDialog : Window
{
    public SettingsDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => DialogFocusHelper.ActivateAndFocus(this, QuickSwitcherHotkeyBox);
    }

    public SettingsDialogResult? Result { get; private set; }

    private void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsDialogViewModel viewModel ||
            !viewModel.TryCreateResult(out SettingsDialogResult? result))
        {
            return;
        }

        Result = result;
        DialogResult = true;
    }

    private void OnHotkeyTextBoxPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (sender is not WpfTextBox textBox)
        {
            return;
        }

        Key key = e.Key == Key.System ? e.SystemKey : e.Key;
        ModifierKeys modifiers = Keyboard.Modifiers;
        if ((key == Key.Back || key == Key.Delete) && modifiers == ModifierKeys.None)
        {
            textBox.Text = string.Empty;
            textBox.GetBindingExpression(WpfTextBox.TextProperty)?.UpdateSource();
            e.Handled = true;
            return;
        }

        if (WpfHotkeyRecorder.IsModifierKey(key))
        {
            e.Handled = true;
            return;
        }

        textBox.Text = WpfHotkeyRecorder.FormatRecordedHotkey(key, modifiers);
        textBox.GetBindingExpression(WpfTextBox.TextProperty)?.UpdateSource();
        e.Handled = true;
    }
}
