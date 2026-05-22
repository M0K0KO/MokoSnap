using System.ComponentModel;
using System.Runtime.CompilerServices;
using MokoSnap.Core.ChromeCapture;

namespace MokoSnap.App.ViewModels;

public sealed class ChromeTabItemViewModel : INotifyPropertyChanged
{
    private bool _isSelected = true;

    public ChromeTabItemViewModel(ChromeTabInfo tab)
    {
        Tab = tab;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ChromeTabInfo Tab { get; }

    public string Title => string.IsNullOrWhiteSpace(Tab.Title) ? Tab.Url : Tab.Title;

    public string Url => Tab.Url;

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
