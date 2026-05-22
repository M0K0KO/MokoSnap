namespace MokoSnap.Core.Startup;

public interface IStartupRegistrationService
{
    bool IsRegistered();

    string? GetRegisteredCommand();

    void SetLaunchOnStartup(bool enabled, bool startMinimized);
}
