using System.ComponentModel;
using System.Runtime.CompilerServices;
using MokoSnap.Core.Running;

namespace MokoSnap.App.ViewModels;

public sealed class CloseWindowCandidateItemViewModel : INotifyPropertyChanged
{
    private bool _isSelected = true;

    public CloseWindowCandidateItemViewModel(CloseWindowCandidate candidate)
    {
        Candidate = candidate;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public CloseWindowCandidate Candidate { get; }

    public string WindowTitle => Candidate.WindowTitle;

    public string ProcessName => Candidate.ProcessName;

    public string ExecutablePath => string.IsNullOrWhiteSpace(Candidate.ExecutablePath)
        ? "Executable path unavailable"
        : Candidate.ExecutablePath;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            OnPropertyChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
