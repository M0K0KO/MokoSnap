using System.Text.Json;
using System.Text.Json.Serialization;

namespace MokoSnap.Core.Storage;

public sealed class FileJsonStorage<T> : IJsonStorage<T>
{
    private static readonly JsonSerializerOptions DefaultOptions = CreateJsonSerializerOptions();

    private readonly Func<T> _createDefault;

    public FileJsonStorage(string filePath, Func<T> createDefault)
    {
        FilePath = filePath;
        _createDefault = createDefault;
    }

    public string FilePath { get; }

    public async Task<T> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(FilePath))
        {
            return _createDefault();
        }

        await using FileStream stream = File.OpenRead(FilePath);
        if (stream.Length == 0)
        {
            return _createDefault();
        }

        T? value = await JsonSerializer.DeserializeAsync<T>(stream, DefaultOptions, cancellationToken);
        return value ?? _createDefault();
    }

    public async Task SaveAsync(T value, CancellationToken cancellationToken = default)
    {
        string? directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using FileStream stream = File.Create(FilePath);
        await JsonSerializer.SerializeAsync(stream, value, DefaultOptions, cancellationToken);
    }

    public static JsonSerializerOptions CreateJsonSerializerOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}
