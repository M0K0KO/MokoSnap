using System.ComponentModel;
using System.Runtime.CompilerServices;
using MokoSnap.Core.Models;

namespace MokoSnap.App.ViewModels;

public sealed class TargetEditorViewModel : INotifyPropertyChanged
{
    private TargetType _type;
    private string _displayName = string.Empty;
    private string _executablePath = string.Empty;
    private string _arguments = string.Empty;
    private string _workingDirectory = string.Empty;
    private int _launchDelayMs;
    private bool _runAsAdmin;
    private WindowPlacementSnapshot? _windowPlacement;
    private string _profileName = string.Empty;
    private bool _openInNewWindow;
    private string _urlsText = string.Empty;
    private string _pageUrlsText = string.Empty;
    private bool _preferDesktopApp = true;
    private string _url = string.Empty;
    private string _path = string.Empty;

    public TargetEditorViewModel(TargetConfig target)
    {
        Type = target.Type;
        DisplayName = target.DisplayName;
        ExecutablePath = target.ExecutablePath;
        Arguments = target.Arguments;
        WorkingDirectory = target.WorkingDirectory;
        LaunchDelayMs = target.LaunchDelayMs;
        RunAsAdmin = target.RunAsAdmin;
        _windowPlacement = target.WindowPlacement?.Clone();
        ProfileName = target.ProfileName;
        OpenInNewWindow = target.OpenInNewWindow;
        UrlsText = JoinLines(target.Urls);
        PageUrlsText = JoinLines(target.PageUrls);
        PreferDesktopApp = target.PreferDesktopApp;
        Url = target.Url;
        Path = target.Path;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Summary => string.IsNullOrWhiteSpace(DisplayName) ? Type.ToString() : DisplayName;

    public string TypeLabel => Type.ToString();

    public string DisplayTitle => string.IsNullOrWhiteSpace(DisplayName) ? $"{Type} target" : DisplayName;

    public string DetailSummary => Type switch
    {
        TargetType.Application => string.IsNullOrWhiteSpace(ExecutablePath) ? "No executable path set" : ExecutablePath,
        TargetType.Chrome => $"{SplitLines(UrlsText).Count} URL(s)",
        TargetType.Notion => $"{SplitLines(PageUrlsText).Count} page(s)",
        TargetType.Url => string.IsNullOrWhiteSpace(Url) ? "No URL set" : Url,
        TargetType.Folder => string.IsNullOrWhiteSpace(Path) ? "No folder path set" : Path,
        _ => string.Empty
    };

    public string MetadataSummary
    {
        get
        {
            List<string> parts = [];
            if (LaunchDelayMs > 0)
            {
                parts.Add($"Delay {LaunchDelayMs} ms");
            }

            if (Type == TargetType.Application && WindowPlacementEnabled)
            {
                parts.Add($"Window placement: {WindowPlacementSummary}");
            }

            return parts.Count == 0 ? "No delay or placement" : string.Join(" | ", parts);
        }
    }

    public TargetType Type
    {
        get => _type;
        set
        {
            if (SetField(ref _type, value))
            {
                OnPropertyChanged(nameof(Summary));
                OnPropertyChanged(nameof(TypeLabel));
                OnPropertyChanged(nameof(DisplayTitle));
                OnPropertyChanged(nameof(DetailSummary));
                OnPropertyChanged(nameof(MetadataSummary));
                OnPropertyChanged(nameof(IsApplication));
                OnPropertyChanged(nameof(IsUrl));
                OnPropertyChanged(nameof(IsFolder));
                OnPropertyChanged(nameof(IsChrome));
                OnPropertyChanged(nameof(IsNotion));
            }
        }
    }

    public string DisplayName
    {
        get => _displayName;
        set
        {
            if (SetField(ref _displayName, value))
            {
                OnPropertyChanged(nameof(Summary));
                OnPropertyChanged(nameof(DisplayTitle));
            }
        }
    }

    public string ExecutablePath
    {
        get => _executablePath;
        set
        {
            if (SetField(ref _executablePath, value))
            {
                OnPropertyChanged(nameof(DetailSummary));
            }
        }
    }

    public string Arguments
    {
        get => _arguments;
        set => SetField(ref _arguments, value);
    }

    public string WorkingDirectory
    {
        get => _workingDirectory;
        set => SetField(ref _workingDirectory, value);
    }

    public int LaunchDelayMs
    {
        get => _launchDelayMs;
        set
        {
            if (SetField(ref _launchDelayMs, value))
            {
                OnPropertyChanged(nameof(MetadataSummary));
            }
        }
    }

    public bool RunAsAdmin
    {
        get => _runAsAdmin;
        set => SetField(ref _runAsAdmin, value);
    }

    public Array WindowPlacementShowStates { get; } = Enum.GetValues(typeof(WindowPlacementShowState));

    public bool HasWindowPlacement => _windowPlacement is not null;

    public bool WindowPlacementEnabled
    {
        get => _windowPlacement?.Enabled == true;
        set
        {
            WindowPlacementSnapshot placement = EnsureWindowPlacement();
            if (placement.Enabled == value)
            {
                return;
            }

            placement.Enabled = value;
            OnWindowPlacementChanged(nameof(WindowPlacementEnabled));
            OnPropertyChanged(nameof(MetadataSummary));
        }
    }

    public WindowPlacementShowState WindowPlacementShowState
    {
        get => _windowPlacement?.ShowState ?? WindowPlacementShowState.Normal;
        set
        {
            WindowPlacementSnapshot placement = EnsureWindowPlacement();
            if (placement.ShowState == value)
            {
                return;
            }

            placement.ShowState = value;
            OnWindowPlacementChanged(nameof(WindowPlacementShowState));
        }
    }

    public int WindowPlacementLeft
    {
        get => _windowPlacement?.Left ?? 0;
        set
        {
            WindowPlacementSnapshot placement = EnsureWindowPlacement();
            if (placement.Left == value)
            {
                return;
            }

            placement.Left = value;
            OnWindowPlacementChanged(nameof(WindowPlacementLeft));
        }
    }

    public int WindowPlacementTop
    {
        get => _windowPlacement?.Top ?? 0;
        set
        {
            WindowPlacementSnapshot placement = EnsureWindowPlacement();
            if (placement.Top == value)
            {
                return;
            }

            placement.Top = value;
            OnWindowPlacementChanged(nameof(WindowPlacementTop));
        }
    }

    public int WindowPlacementWidth
    {
        get => _windowPlacement?.Width ?? 0;
        set
        {
            WindowPlacementSnapshot placement = EnsureWindowPlacement();
            int width = Math.Max(1, value);
            if (placement.Width == width)
            {
                return;
            }

            placement.Width = width;
            OnWindowPlacementChanged(nameof(WindowPlacementWidth));
        }
    }

    public int WindowPlacementHeight
    {
        get => _windowPlacement?.Height ?? 0;
        set
        {
            WindowPlacementSnapshot placement = EnsureWindowPlacement();
            int height = Math.Max(1, value);
            if (placement.Height == height)
            {
                return;
            }

            placement.Height = height;
            OnWindowPlacementChanged(nameof(WindowPlacementHeight));
        }
    }

    public string WindowPlacementMonitorDeviceName
    {
        get => _windowPlacement?.MonitorDeviceName ?? string.Empty;
        set
        {
            WindowPlacementSnapshot placement = EnsureWindowPlacement();
            string monitorDeviceName = value.Trim();
            if (placement.MonitorDeviceName == monitorDeviceName)
            {
                return;
            }

            placement.MonitorDeviceName = monitorDeviceName;
            OnWindowPlacementChanged(nameof(WindowPlacementMonitorDeviceName));
        }
    }

    public bool WindowPlacementWasProbablySnapped => _windowPlacement?.WasProbablySnapped == true;

    public string WindowPlacementSummary => _windowPlacement is null
        ? "No placement captured"
        : $"{_windowPlacement.ShowState} {_windowPlacement.Left},{_windowPlacement.Top} {_windowPlacement.Width}x{_windowPlacement.Height}";

    public string ProfileName
    {
        get => _profileName;
        set => SetField(ref _profileName, value);
    }

    public bool OpenInNewWindow
    {
        get => _openInNewWindow;
        set => SetField(ref _openInNewWindow, value);
    }

    public string UrlsText
    {
        get => _urlsText;
        set
        {
            if (SetField(ref _urlsText, value))
            {
                OnPropertyChanged(nameof(DetailSummary));
            }
        }
    }

    public string PageUrlsText
    {
        get => _pageUrlsText;
        set
        {
            if (SetField(ref _pageUrlsText, value))
            {
                OnPropertyChanged(nameof(DetailSummary));
            }
        }
    }

    public bool PreferDesktopApp
    {
        get => _preferDesktopApp;
        set => SetField(ref _preferDesktopApp, value);
    }

    public string Url
    {
        get => _url;
        set
        {
            if (SetField(ref _url, value))
            {
                OnPropertyChanged(nameof(DetailSummary));
            }
        }
    }

    public string Path
    {
        get => _path;
        set
        {
            if (SetField(ref _path, value))
            {
                OnPropertyChanged(nameof(DetailSummary));
            }
        }
    }

    public bool IsApplication => Type == TargetType.Application;

    public bool IsUrl => Type == TargetType.Url;

    public bool IsFolder => Type == TargetType.Folder;

    public bool IsChrome => Type == TargetType.Chrome;

    public bool IsNotion => Type == TargetType.Notion;

    public TargetConfig ToTarget()
    {
        return new TargetConfig
        {
            Type = Type,
            DisplayName = DisplayName.Trim(),
            ExecutablePath = ExecutablePath.Trim(),
            Arguments = Arguments.Trim(),
            WorkingDirectory = WorkingDirectory.Trim(),
            LaunchDelayMs = Math.Max(0, LaunchDelayMs),
            RunAsAdmin = RunAsAdmin,
            WindowPlacement = Type == TargetType.Application ? _windowPlacement?.Clone() : null,
            ProfileName = ProfileName.Trim(),
            OpenInNewWindow = OpenInNewWindow,
            Urls = SplitLines(UrlsText),
            PageUrls = SplitLines(PageUrlsText),
            PreferDesktopApp = PreferDesktopApp,
            Url = Url.Trim(),
            Path = Path.Trim()
        };
    }

    private static string JoinLines(IEnumerable<string> values)
    {
        return string.Join(Environment.NewLine, values);
    }

    private static List<string> SplitLines(string value)
    {
        return value
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private WindowPlacementSnapshot EnsureWindowPlacement()
    {
        if (_windowPlacement is not null)
        {
            return _windowPlacement;
        }

        _windowPlacement = new WindowPlacementSnapshot
        {
            Enabled = true,
            ShowState = WindowPlacementShowState.Normal,
            Width = 800,
            Height = 600
        };
        OnPropertyChanged(nameof(HasWindowPlacement));
        OnPropertyChanged(nameof(WindowPlacementSummary));
        return _windowPlacement;
    }

    private void OnWindowPlacementChanged(string propertyName)
    {
        OnPropertyChanged(propertyName);
        OnPropertyChanged(nameof(HasWindowPlacement));
        OnPropertyChanged(nameof(WindowPlacementSummary));
        OnPropertyChanged(nameof(MetadataSummary));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
