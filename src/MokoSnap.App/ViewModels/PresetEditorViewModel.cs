using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MokoSnap.Core.Capture;
using MokoSnap.Core.Hotkeys;
using MokoSnap.Core.Models;

namespace MokoSnap.App.ViewModels;

public sealed class PresetEditorViewModel : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _hotkeyText = string.Empty;
    private ClosePolicy _closePolicy;
    private CloseConfirmationPolicy _closeConfirmationPolicy = CloseConfirmationPolicy.AlwaysConfirm;
    private TargetEditorViewModel? _selectedTarget;

    public PresetEditorViewModel(Preset preset)
    {
        Id = preset.Id;
        Name = preset.Name;
        Description = preset.Description;
        HotkeyText = HotkeyGestureFormatter.Format(preset.Hotkey);
        ClosePolicy = preset.ClosePolicy;
        CloseConfirmationPolicy = preset.CloseConfirmationPolicy;
        foreach (TargetConfig target in preset.Targets)
        {
            Targets.Add(new TargetEditorViewModel(target));
        }

        AddTargetCommand = new RelayCommand(AddTarget);
        DeleteTargetCommand = new RelayCommand(DeleteTarget, () => SelectedTarget is not null);
        MoveTargetUpCommand = new RelayCommand(MoveTargetUp, CanMoveTargetUp);
        MoveTargetDownCommand = new RelayCommand(MoveTargetDown, CanMoveTargetDown);
        SelectedTarget = Targets.FirstOrDefault();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; }

    public ObservableCollection<TargetEditorViewModel> Targets { get; } = [];

    public Array TargetTypes { get; } = Enum.GetValues(typeof(TargetType));

    public RelayCommand AddTargetCommand { get; }

    public RelayCommand DeleteTargetCommand { get; }

    public RelayCommand MoveTargetUpCommand { get; }

    public RelayCommand MoveTargetDownCommand { get; }

    public TargetEditorViewModel? SelectedTarget
    {
        get => _selectedTarget;
        set
        {
            if (_selectedTarget == value)
            {
                return;
            }

            _selectedTarget = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTarget)));
            RaiseTargetCommandCanExecuteChanged();
        }
    }

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    public string HotkeyText
    {
        get => _hotkeyText;
        set => SetField(ref _hotkeyText, value);
    }

    public ClosePolicy ClosePolicy
    {
        get => _closePolicy;
        set => SetField(ref _closePolicy, value);
    }

    public CloseConfirmationPolicy CloseConfirmationPolicy
    {
        get => _closeConfirmationPolicy;
        set => SetField(ref _closeConfirmationPolicy, value);
    }

    public Preset ToPreset()
    {
        return new Preset
        {
            Id = Id,
            Name = Name.Trim(),
            Description = Description.Trim(),
            Hotkey = HotkeyGestureFormatter.Parse(HotkeyText),
            ClosePolicy = ClosePolicy,
            CloseConfirmationPolicy = CloseConfirmationPolicy,
            Targets = Targets.Select(target => target.ToTarget()).ToList()
        };
    }

    public void AppendCapturedApps(IEnumerable<CapturedWindowApp> capturedApps)
    {
        foreach (CapturedWindowApp app in capturedApps.Where(app => app.IsAvailable))
        {
            TargetEditorViewModel target = new(app.ToApplicationTarget());
            Targets.Add(target);
            SelectedTarget = target;
        }

        RaiseTargetCommandCanExecuteChanged();
    }

    public void AppendChromeTarget(TargetConfig targetConfig)
    {
        TargetEditorViewModel target = new(targetConfig);
        Targets.Add(target);
        SelectedTarget = target;
        RaiseTargetCommandCanExecuteChanged();
    }

    private void AddTarget()
    {
        TargetEditorViewModel target = new(new TargetConfig
        {
            Type = TargetType.Url,
            DisplayName = "New Target"
        });

        Targets.Add(target);
        SelectedTarget = target;
        RaiseTargetCommandCanExecuteChanged();
    }

    private void DeleteTarget()
    {
        if (SelectedTarget is null)
        {
            return;
        }

        int index = Targets.IndexOf(SelectedTarget);
        Targets.Remove(SelectedTarget);
        SelectedTarget = Targets.Count == 0 ? null : Targets[Math.Min(index, Targets.Count - 1)];
        RaiseTargetCommandCanExecuteChanged();
    }

    private void MoveTargetUp()
    {
        if (SelectedTarget is null)
        {
            return;
        }

        int index = Targets.IndexOf(SelectedTarget);
        if (index <= 0)
        {
            return;
        }

        Targets.Move(index, index - 1);
        RaiseTargetCommandCanExecuteChanged();
    }

    private void MoveTargetDown()
    {
        if (SelectedTarget is null)
        {
            return;
        }

        int index = Targets.IndexOf(SelectedTarget);
        if (index < 0 || index >= Targets.Count - 1)
        {
            return;
        }

        Targets.Move(index, index + 1);
        RaiseTargetCommandCanExecuteChanged();
    }

    private bool CanMoveTargetUp()
    {
        return SelectedTarget is not null && Targets.IndexOf(SelectedTarget) > 0;
    }

    private bool CanMoveTargetDown()
    {
        return SelectedTarget is not null &&
            Targets.IndexOf(SelectedTarget) >= 0 &&
            Targets.IndexOf(SelectedTarget) < Targets.Count - 1;
    }

    private void RaiseTargetCommandCanExecuteChanged()
    {
        DeleteTargetCommand.RaiseCanExecuteChanged();
        MoveTargetUpCommand.RaiseCanExecuteChanged();
        MoveTargetDownCommand.RaiseCanExecuteChanged();
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
