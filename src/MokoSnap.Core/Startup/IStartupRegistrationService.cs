namespace MokoSnap.Core.Startup;

public interface IStartupRegistrationService
{
    bool IsRegistered();

    string? GetRegisteredCommand();

    string GetExpectedCommand(bool startMinimized);

    void SetLaunchOnStartup(bool enabled, bool startMinimized);
}
