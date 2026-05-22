\# Task: Run preset UI



\## Goal



Add UI support for running the selected preset.



This task wires the existing Core PresetRunnerService to the Windows target launcher and shows run results to the user.



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- src/MokoSnap.Platform.Windows/ only if tiny integration code is required

\- src/MokoSnap.Core/ only if a tiny interface adjustment is required

\- src/MokoSnap.Tests/ only if useful

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not implement global hotkeys.

\- Do not implement command palette.

\- Do not implement visible window closing yet.

\- Do not implement Chrome tab capture.

\- Do not implement tray or startup behavior.

\- Do not add third-party UI/MVVM libraries.

\- Do not refactor unrelated UI.



\## Assumptions



\- PresetRunnerService already exists in Core.

\- WindowsTargetLauncher already implements or can be adapted to the existing target launcher abstraction.

\- Visible window closing is not implemented yet.

\- If a preset uses ClosePolicy.CloseVisibleWindowsOnly, the UI should block execution with a clear message for now.

\- ClosePolicy.None presets can run now.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



1\. Add a `Run` button for the selected preset.

2\. The button should be disabled when no preset is selected.

3\. Running a preset should use:

&#x20;  - PresetRunnerService

&#x20;  - WindowsTargetLauncher

&#x20;  - SystemLaunchDelay or existing production delay implementation

4\. If selected preset has `ClosePolicy.None`, run its targets.

5\. If selected preset has `ClosePolicy.CloseVisibleWindowsOnly`, do not run yet. Show a message:

&#x20;  - "Close visible windows is not implemented yet. Set close policy to None or implement the close windows task first."

6\. While running:

&#x20;  - prevent duplicate runs

&#x20;  - keep UI responsive

&#x20;  - show basic running state

7\. After running:

&#x20;  - show a result dialog or message area with:

&#x20;    - successful targets

&#x20;    - failed targets

&#x20;    - error messages

8\. Failed target launches must not crash the app.

9\. Existing preset CRUD and capture features must keep working.

10\. Existing tests must continue passing.



\## Verification



Run:



```powershell

./scripts/verify.ps1

