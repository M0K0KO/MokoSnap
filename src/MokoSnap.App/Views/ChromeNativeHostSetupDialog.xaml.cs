using System.Windows;
using MokoSnap.App.Services;

namespace MokoSnap.App.Views;

public partial class ChromeNativeHostSetupDialog : Window
{
    public ChromeNativeHostSetupDialog()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        DialogFocusHelper.ActivateAndFocus(this, this);
    }
}
