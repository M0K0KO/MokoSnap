\# Task: Hotkey and command palette



\## Goal



Implement high-quality keyboard-driven preset switching.



MokoSnap should support:

\- global quick switcher hotkey

\- per-preset global hotkeys

\- command-palette style preset search

\- hotkey conflict detection



\## Scope



Allowed changes:



\- src/MokoSnap.Core/

\- src/MokoSnap.Platform.Windows/

\- src/MokoSnap.App/

\- src/MokoSnap.Tests/

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not implement Chrome tab capture.

\- Do not implement tray/startup.

\- Do not add third-party hotkey libraries.

\- Do not refactor unrelated UI.

\- Do not change visible window closing behavior except to trigger existing run flow.



\## Assumptions



\- Windows global hotkeys use RegisterHotKey.

\- Per-preset hotkeys are stored in existing HotkeyGesture data.

\- The default command palette hotkey is Ctrl+Alt+Space.

\- The command palette should allow running presets without mouse.

\- Hotkey registration failure should not crash the app.

\- Existing preset run flow should be reused.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



1\. Add Windows global hotkey service in `MokoSnap.Platform.Windows`.

2\. Use Win32 `RegisterHotKey` / `UnregisterHotKey`.

3\. Support registering:

&#x20;  - global command palette hotkey

&#x20;  - per-preset hotkeys

4\. Add result data for hotkey registration:

&#x20;  - success

&#x20;  - failed gesture

&#x20;  - error message

&#x20;  - possibly conflicting preset name

5\. Add hotkey validation:

&#x20;  - empty hotkey allowed

&#x20;  - duplicate preset hotkeys are invalid

&#x20;  - single non-modifier key is invalid

&#x20;  - Alt+F4 is invalid

&#x20;  - warn or reject common shortcuts like Ctrl+C / Ctrl+V if simple

6\. Add command palette UI:

&#x20;  - opens with Ctrl+Alt+Space

&#x20;  - centered small window/dialog

&#x20;  - search box focused immediately

&#x20;  - list presets filtered by name

&#x20;  - Enter runs selected preset

&#x20;  - Esc closes palette

&#x20;  - Up/Down changes selected item

7\. Add Settings or simple location for global quick switcher hotkey:

&#x20;  - default Ctrl+Alt+Space is acceptable for this task

&#x20;  - full settings UI can be deferred

8\. Wire per-preset hotkeys:

&#x20;  - when app starts, register valid preset hotkeys

&#x20;  - when presets are saved/changed, refresh registrations

&#x20;  - when hotkey pressed, run corresponding preset

9\. Prevent duplicate runs:

&#x20;  - if a preset is already running, ignore or show simple message

10\. Keep UI responsive.

11\. Existing Run button behavior must keep working.

12\. Existing CloseVisibleWindowsOnly flow must work when launched by hotkey.

13\. Add unit tests for hotkey parsing/validation/conflict logic.

14\. Avoid tests that require real global hotkey registration.



\## Verification



Run:



```powershell

./scripts/verify.ps1

