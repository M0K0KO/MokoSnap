namespace MokoSnap.Core.Storage;

public interface IJsonStorage<T>
{
    Task<T> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(T value, CancellationToken cancellationToken = default);
}
