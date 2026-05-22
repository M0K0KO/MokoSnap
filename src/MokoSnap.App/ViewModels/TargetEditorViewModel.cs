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

    public TargetType Type
    {
        get => _type;
        set
        {
            if (SetField(ref _type, value))
            {
                OnPropertyChanged(nameof(Summary));
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
            }
        }
    }

    public string ExecutablePath
    {
        get => _executablePath;
        set => SetField(ref _executablePath, value);
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
        set => SetField(ref _launchDelayMs, value);
    }

    public bool RunAsAdmin
    {
        get => _runAsAdmin;
        set => SetField(ref _runAsAdmin, value);
    }

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
        set => SetField(ref _urlsText, value);
    }

    public string PageUrlsText
    {
        get => _pageUrlsText;
        set => SetField(ref _pageUrlsText, value);
    }

    public bool PreferDesktopApp
    {
        get => _preferDesktopApp;
        set => SetField(ref _preferDesktopApp, value);
    }

    public string Url
    {
        get => _url;
        set => SetField(ref _url, value);
    }

    public string Path
    {
        get => _path;
        set => SetField(ref _path, value);
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
