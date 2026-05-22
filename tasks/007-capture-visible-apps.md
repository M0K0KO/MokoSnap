\# Task: Capture visible apps



\## Goal



Implement a feature that captures currently visible top-level application windows and adds selected apps as targets to a preset.



This task captures visible apps only. It does not capture Chrome tabs, browser URLs, Notion page state, or app internal state.



\## Scope



Allowed changes:



\- src/MokoSnap.Core/

\- src/MokoSnap.Platform.Windows/

\- src/MokoSnap.App/

\- src/MokoSnap.Tests/

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not implement Chrome tab capture.

\- Do not implement Chrome Extension code.

\- Do not implement preset execution.

\- Do not implement global hotkeys.

\- Do not implement visible window closing.

\- Do not implement app state restore.

\- Do not add third-party UI/MVVM libraries.



\## Assumptions



\- Capture means visible top-level windows only.

\- Captured apps become Application targets.

\- Captured targets may require manual editing later.

\- Explorer windows are excluded by default but can be included through an option.

\- MokoSnap itself must never be captured.

\- Background processes are ignored.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



1\. Add a Windows platform service for enumerating visible top-level windows.

2\. Implement a model such as `CapturedWindowApp` containing:

&#x20;  - window title

&#x20;  - process name

&#x20;  - executable path if available

&#x20;  - process id

&#x20;  - window handle or stable identifier if needed internally

&#x20;  - whether it is Explorer

3\. Filtering rules:

&#x20;  - include visible top-level windows

&#x20;  - exclude empty-title windows

&#x20;  - exclude MokoSnap

&#x20;  - exclude system UI windows

&#x20;  - exclude Explorer by default

&#x20;  - allow Explorer inclusion through an option

4\. Add an abstraction in Core if needed:

&#x20;  - `IVisibleAppCaptureService`

5\. Add UI button:

&#x20;  - `Capture Current Apps`

6\. When clicked:

&#x20;  - enumerate visible apps

&#x20;  - show a preview dialog/list

&#x20;  - allow user to check/uncheck apps

&#x20;  - allow option: include Explorer windows

7\. On confirmation:

&#x20;  - convert selected captured apps into Application targets

&#x20;  - append them to the selected preset's target list

&#x20;  - preserve existing targets

8\. Captured Application target fields:

&#x20;  - display name = process/window friendly name

&#x20;  - executable path = captured executable path

&#x20;  - arguments = empty

&#x20;  - working directory = empty

&#x20;  - launch delay ms = 0

&#x20;  - run as admin = false

9\. If executable path cannot be found:

&#x20;  - show the item as unavailable or disabled

&#x20;  - do not create an invalid target unless user explicitly allows it

10\. Add tests for filtering/conversion logic where possible.

11\. Avoid tests that depend on real desktop windows.



\## Verification



Run:



```powershell

./scripts/verify.ps1

