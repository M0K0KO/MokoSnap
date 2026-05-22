using System.Windows;

namespace MokoSnap.App.Services;

public sealed class MessageBoxConfirmationService : IConfirmationService
{
    public bool Confirm(string message, string title)
    {
        return System.Windows.MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }
}
