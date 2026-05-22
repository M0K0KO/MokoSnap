using MokoSnap.Core.Running;

namespace MokoSnap.Platform.Windows.Closing;

public interface IVisibleWindowCloseCandidateProvider
{
    Task<IReadOnlyList<CloseWindowCandidate>> GetCandidatesAsync(
        bool includeExplorer,
        CancellationToken cancellationToken = default);
}
