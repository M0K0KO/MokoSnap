\# Task: Window placement memory



\## Goal



Add window placement memory to captured Application targets.



MokoSnap should remember the current visible window's position, size, monitor, and show state when capturing apps. When running a preset, MokoSnap should try to restore the launched app's window placement.



This task does not need to restore true Windows Snap Layout metadata. It should restore the resulting window rectangle/state.



\## Scope



Allowed changes:



\- src/MokoSnap.Core/

\- src/MokoSnap.Platform.Windows/

\- src/MokoSnap.App/

\- src/MokoSnap.Tests/

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not implement Chrome tab capture.

\- Do not implement global hotkeys.

\- Do not implement command palette.

\- Do not implement tray/startup.

\- Do not implement visible window closing.

\- Do not force exclusive fullscreen modes.

\- Do not use third-party window management libraries.



\## Assumptions



\- Window placement means position, size, monitor, and show state.

\- Snap Layout metadata is not officially restored; MokoSnap restores the snapped window's resulting rectangle.

\- True game fullscreen state is app-internal and not guaranteed to restore.

\- Placement restore applies only to Application targets.

\- If placement restore fails, target launch should still count as launched but include a warning.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



1\. Add a core model:

&#x20;  - `WindowPlacementSnapshot`

2\. The snapshot should contain:

&#x20;  - enabled

&#x20;  - show state: Normal / Maximized / Minimized

&#x20;  - left

&#x20;  - top

&#x20;  - width

&#x20;  - height

&#x20;  - monitor device name if available

&#x20;  - was probably snapped if cheaply detectable

3\. Add optional `WindowPlacementSnapshot` to Application targets.

4\. Update visible app capture:

&#x20;  - capture window placement for each visible app

&#x20;  - default `enabled = true`

&#x20;  - allow user to uncheck `Remember window position`

5\. Update target editor UI for Application targets:

&#x20;  - show whether placement restore is enabled

&#x20;  - allow toggling placement restore

&#x20;  - display captured rect/state in a simple read-only or editable form

6\. Add Windows platform placement service:

&#x20;  - capture placement from HWND

&#x20;  - restore placement to HWND

7\. Use Win32 APIs:

&#x20;  - GetWindowPlacement / SetWindowPlacement where appropriate

&#x20;  - DwmGetWindowAttribute(DWMWA\_EXTENDED\_FRAME\_BOUNDS) or equivalent for visible bounds if useful

&#x20;  - SetWindowPos only where appropriate

8\. Update Windows target launch flow:

&#x20;  - after launching an Application target with placement enabled, try to find a matching visible window

&#x20;  - apply placement

&#x20;  - keep UI responsive

&#x20;  - timeout if window is not found

9\. Matching should be simple:

&#x20;  - prefer process id from launched process if available

&#x20;  - fallback to executable path/process name matching

&#x20;  - do not overengineer fuzzy matching

10\. If monitor no longer exists:

&#x20;  - clamp or move rect to current primary monitor/work area

&#x20;  - do not place the window completely off-screen

11\. Add tests for:

&#x20;  - Application target JSON round-trip with placement

&#x20;  - captured app to Application target conversion includes placement

&#x20;  - monitor/rect clamping logic if implemented in Core

12\. Avoid tests that depend on real desktop windows.



\## Verification



Run:



```powershell

./scripts/verify.ps1

