using MokoSnap.Core.Models;
using MokoSnap.Core.Storage;

namespace MokoSnap.Tests;

public class ModelDefaultsTests
{
    [Fact]
    public void PresetUsesSafeDefaults()
    {
        Preset preset = new();

        Assert.False(string.IsNullOrWhiteSpace(preset.Id));
        Assert.Equal(string.Empty, preset.Name);
        Assert.Equal(string.Empty, preset.Description);
        Assert.Null(preset.Hotkey);
        Assert.Equal(ClosePolicy.None, preset.ClosePolicy);
        Assert.Equal(CloseConfirmationPolicy.AlwaysConfirm, preset.CloseConfirmationPolicy);
        Assert.Empty(preset.Targets);
    }

    [Fact]
    public void AppSettingsStartsEmpty()
    {
        AppSettings settings = AppSettings.CreateDefault();

        Assert.Equal("1", settings.Version);
        Assert.Empty(settings.Presets);
    }

    [Fact]
    public void LaunchHistoryStartsEmpty()
    {
        LaunchHistory history = LaunchHistory.CreateDefault();

        Assert.Empty(history.Entries);
    }

    [Fact]
    public void DefaultStoragePathsUseAppDataMokoSnapDirectory()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        Assert.Equal(System.IO.Path.Combine(appData, "MokoSnap"), MokoSnapStoragePaths.AppDataDirectory);
        Assert.Equal(
            System.IO.Path.Combine(appData, "MokoSnap", "appsettings.json"),
            MokoSnapStoragePaths.AppSettingsPath);
        Assert.Equal(
            System.IO.Path.Combine(appData, "MokoSnap", "launch-history.json"),
            MokoSnapStoragePaths.LaunchHistoryPath);
    }
}
