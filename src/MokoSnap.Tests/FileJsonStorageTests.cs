using MokoSnap.Core.Models;
using MokoSnap.Core.Storage;

namespace MokoSnap.Tests;

public class FileJsonStorageTests
{
    [Fact]
    public async Task LoadAsyncReturnsDefaultWhenFileDoesNotExist()
    {
        using TempDirectory tempDirectory = new();
        string path = System.IO.Path.Combine(tempDirectory.Path, "settings.json");
        FileJsonStorage<AppSettings> storage = new(path, AppSettings.CreateDefault);

        AppSettings settings = await storage.LoadAsync();

        Assert.Equal("1", settings.Version);
        Assert.Empty(settings.Presets);
    }

    [Fact]
    public async Task SaveAsyncCreatesDirectoryAndLoadAsyncReadsSavedValue()
    {
        using TempDirectory tempDirectory = new();
        string path = System.IO.Path.Combine(tempDirectory.Path, "nested", "settings.json");
        FileJsonStorage<AppSettings> storage = new(path, AppSettings.CreateDefault);
        AppSettings settings = new()
        {
            Presets =
            [
                new Preset
                {
                    Id = "study",
                    Name = "Study",
                    Targets =
                    [
                        new TargetConfig
                        {
                            Type = TargetType.Url,
                            DisplayName = "Course",
                            Url = "https://example.com/course"
                        }
                    ]
                }
            ]
        };

        await storage.SaveAsync(settings);
        AppSettings loaded = await storage.LoadAsync();

        Assert.True(File.Exists(path));
        Preset preset = Assert.Single(loaded.Presets);
        Assert.Equal("study", preset.Id);
        Assert.Equal("Study", preset.Name);
        TargetConfig target = Assert.Single(preset.Targets);
        Assert.Equal(TargetType.Url, target.Type);
        Assert.Equal("https://example.com/course", target.Url);
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"MokoSnapTests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
