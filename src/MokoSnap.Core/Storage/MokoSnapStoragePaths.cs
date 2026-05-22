using MokoSnap.Core.Models;

namespace MokoSnap.Core.Storage;

public static class MokoSnapStoragePaths
{
    public static string AppDataDirectory { get; } =
        System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MokoSnap");

    public static string AppSettingsPath { get; } =
        System.IO.Path.Combine(AppDataDirectory, "appsettings.json");

    public static string LaunchHistoryPath { get; } =
        System.IO.Path.Combine(AppDataDirectory, "launch-history.json");

    public static FileJsonStorage<AppSettings> CreateAppSettingsStorage()
    {
        return new FileJsonStorage<AppSettings>(AppSettingsPath, AppSettings.CreateDefault);
    }

    public static FileJsonStorage<LaunchHistory> CreateLaunchHistoryStorage()
    {
        return new FileJsonStorage<LaunchHistory>(LaunchHistoryPath, LaunchHistory.CreateDefault);
    }
}
