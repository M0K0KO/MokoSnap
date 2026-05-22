using Microsoft.Win32;
using MokoSnap.Core.Startup;

namespace MokoSnap.Platform.Windows.Startup;

public sealed class WindowsStartupRegistrationService : IStartupRegistrationService
{
    private const string RunSubKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "MokoSnap";
    private readonly string _executablePath;

    public WindowsStartupRegistrationService(string? executablePath = null)
    {
        _executablePath = string.IsNullOrWhiteSpace(executablePath)
            ? Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "MokoSnap.App.exe")
            : executablePath;
    }

    public bool IsRegistered()
    {
#pragma warning disable CA1416
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunSubKey);
        return key?.GetValue(ValueName) is string;
#pragma warning restore CA1416
    }

    public string? GetRegisteredCommand()
    {
#pragma warning disable CA1416
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunSubKey);
        return key?.GetValue(ValueName) as string;
#pragma warning restore CA1416
    }

    public void SetLaunchOnStartup(bool enabled, bool startMinimized)
    {
#pragma warning disable CA1416
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(RunSubKey)
            ?? throw new InvalidOperationException("Could not open the HKCU startup Run key.");
        if (enabled)
        {
            key.SetValue(ValueName, StartupCommand.Build(_executablePath, startMinimized), RegistryValueKind.String);
            return;
        }

        key.DeleteValue(ValueName, false);
#pragma warning restore CA1416
    }
}
