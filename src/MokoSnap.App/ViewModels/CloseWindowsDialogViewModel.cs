using System.Collections.ObjectModel;
using MokoSnap.Core.Running;

namespace MokoSnap.App.ViewModels;

public sealed class CloseWindowsDialogViewModel
{
    public CloseWindowsDialogViewModel(IReadOnlyList<CloseWindowCandidate> candidates)
    {
        foreach (CloseWindowCandidate candidate in candidates)
        {
            Windows.Add(new CloseWindowCandidateItemViewModel(candidate));
        }
    }

    public ObservableCollection<CloseWindowCandidateItemViewModel> Windows { get; } = [];

    public IReadOnlyList<long> SelectedWindowHandles =>
        Windows
            .Where(window => window.IsSelected)
            .Select(window => window.Candidate.WindowHandle)
            .ToList();
}
