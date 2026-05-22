namespace MokoSnap.App.Services;

public interface IConfirmationService
{
    bool Confirm(string message, string title);
}
