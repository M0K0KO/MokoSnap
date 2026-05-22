\# MokoSnap Architecture



\## Projects



\### MokoSnap.Core



Contains:



\- Models

\- Preset validation

\- Preset runner orchestration

\- JSON storage abstractions

\- Result models



Must not reference WPF or Win32 directly.



\### MokoSnap.Platform.Windows



Contains:



\- Win32 window enumeration

\- Global hotkey registration

\- Startup registration

\- Process launching

\- Visible window closing

\- Native Messaging host support



\### MokoSnap.App



Contains:



\- WPF views

\- ViewModels

\- Tray integration

\- Dialogs



\### MokoSnap.Tests



Contains:



\- Unit tests for Core

\- Fake platform services

\- Storage tests

\- Preset runner tests



\## Key Interfaces



```csharp

public interface IProcessLauncher

{

&#x20;   Task<LaunchResult> LaunchAsync(LaunchRequest request, CancellationToken ct);

}



public interface IWindowEnumerator

{

&#x20;   IReadOnlyList<VisibleWindowInfo> GetVisibleWindows();

}



public interface IVisibleWindowCloser

{

&#x20;   Task<CloseWindowsResult> CloseVisibleWindowsAsync(CloseVisibleWindowsRequest request, CancellationToken ct);

}



public interface IHotkeyService

{

&#x20;   HotkeyRegistrationResult RegisterHotkey(PresetId presetId, HotkeyGesture gesture);

&#x20;   void UnregisterHotkey(PresetId presetId);

}



public interface IChromeTabCaptureService

{

&#x20;   Task<IReadOnlyList<ChromeWindowInfo>> CaptureAllWindowsAndTabsAsync(CancellationToken ct);

}



public interface INotionLauncher

{

&#x20;   Task<LaunchResult> OpenPagesAsync(IReadOnlyList<string> urls, bool preferDesktopApp, CancellationToken ct);

}

