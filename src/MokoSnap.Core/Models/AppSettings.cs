namespace MokoSnap.Core.Models;

public sealed class AppSettings
{
    public string Version { get; set; } = "1";

    public List<Preset> Presets { get; set; } = [];

    public static AppSettings CreateDefault()
    {
        return new AppSettings();
    }
}
