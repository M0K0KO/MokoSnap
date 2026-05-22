\# Task: Preset runner core



\## Goal



Implement the core preset execution orchestration without using WPF, Win32, or real process launching.



\## Scope



Allowed changes:



\- src/MokoSnap.Core/

\- src/MokoSnap.Tests/

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not modify WPF UI.

\- Do not implement real process launching.

\- Do not implement Win32 window enumeration.

\- Do not implement hotkeys.

\- Do not implement Chrome extension code.

\- Do not implement Notion desktop detection.



\## Assumptions



\- PresetRunnerService belongs in Core.

\- Platform-specific behavior must be injected through interfaces.

\- Tests should use fake services.

\- Failed target launches should not stop remaining targets.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



1\. Implement `PresetRunnerService`.

2\. Implement platform abstraction interfaces needed by the runner:

&#x20;  - `ITargetLauncher`

&#x20;  - `IVisibleWindowCloser`

3\. Implement result models:

&#x20;  - `PresetRunResult`

&#x20;  - `TargetRunResult`

&#x20;  - `CloseWindowsResult`

4\. If `ClosePolicy.None`, do not call `IVisibleWindowCloser`.

5\. If `ClosePolicy.CloseVisibleWindowsOnly`, call `IVisibleWindowCloser` before launching targets.

6\. Launch targets in preset order.

7\. Respect each target's `LaunchDelayMs`.

8\. If a target fails, record failure and continue launching remaining targets.

9\. Return a complete `PresetRunResult`.

10\. Add unit tests for:

&#x20;   - None close policy does not call window closer

&#x20;   - CloseVisibleWindowsOnly calls window closer

&#x20;   - targets launch in order

&#x20;   - failed target does not stop later targets

&#x20;   - launch delay is respected in a testable way, preferably by injecting delay abstraction instead of using real time sleeps



\## Verification



Run:



```powershell

./scripts/verify.ps1

