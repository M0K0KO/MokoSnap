\# Task: Visible window closing



\## Goal



Implement ClosePolicy.CloseVisibleWindowsOnly.



When a preset uses this policy, MokoSnap should close currently visible top-level application windows before launching preset targets.



This task must close visible windows only. It must not kill arbitrary background processes.



\## Scope



Allowed changes:



\- src/MokoSnap.Core/

\- src/MokoSnap.Platform.Windows/

\- src/MokoSnap.App/

\- src/MokoSnap.Tests/

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not implement global hotkeys.

\- Do not implement command palette.

\- Do not implement Chrome tab capture.

\- Do not implement tray/startup.

\- Do not add third-party UI/MVVM libraries.

\- Do not kill arbitrary background processes.

\- Do not force-kill processes in this task.



\## Assumptions



\- CloseVisibleWindowsOnly means closing visible top-level windows, not killing processes.

\- MokoSnap must never close itself.

\- Explorer windows are excluded by default.

\- Explorer inclusion is optional if existing UI supports it.

\- CloseConfirmationPolicy.AlwaysConfirm shows a preview dialog before closing.

\- CloseConfirmationPolicy.SkipConfirmation closes candidates without preview confirmation.

\- Closing happens before launching preset targets.

\- After closing, preset targets launch normally.

\- Existing window placement restore behavior should continue working after launch.

\- Failed window closes should be reported but should not crash the app.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



1\. Implement a Windows visible window closer in `MokoSnap.Platform.Windows`.

2\. Reuse existing visible window enumeration/filtering logic from capture where possible.

3\. Add or reuse result models for:

&#x20;  - candidate windows

&#x20;  - closed windows

&#x20;  - failed windows

&#x20;  - skipped windows

&#x20;  - canceled close operation

4\. Closing behavior:

&#x20;  - send graceful close request to each selected candidate window

&#x20;  - use `WM\_CLOSE`

&#x20;  - wait a short timeout

&#x20;  - detect whether the window disappeared

&#x20;  - do not force kill

5\. Filtering:

&#x20;  - exclude MokoSnap

&#x20;  - exclude empty-title windows

&#x20;  - exclude invisible windows

&#x20;  - exclude system UI windows

&#x20;  - exclude Explorer by default

6\. UI behavior:

&#x20;  - If `CloseConfirmationPolicy.AlwaysConfirm`:

&#x20;    - show preview dialog of windows to close

&#x20;    - allow user to cancel

&#x20;    - allow user to check/uncheck candidates if simple

&#x20;    - if user cancels, do not launch targets

&#x20;  - If `CloseConfirmationPolicy.SkipConfirmation`:

&#x20;    - close candidates without preview confirmation

&#x20;    - still exclude Explorer by default

7\. Wire into Run preset flow:

&#x20;  - close visible windows first

&#x20;  - then launch targets

&#x20;  - show close results together with target run results

8\. Keep UI responsive.

9\. Existing placement restore must still run after target launch.

10\. Add tests for filtering/orchestration behavior where possible.

11\. Avoid tests that depend on real desktop windows.



\## Verification



Run:



```powershell

./scripts/verify.ps1

