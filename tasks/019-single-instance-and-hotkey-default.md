\# Task: Single instance and default hotkey change



\## Goal



Fix two usability problems:



1\. Prevent multiple MokoSnap instances from running at the same time.

2\. Change the default Quick Switcher hotkey away from Ctrl+Alt+Space because it conflicts with Claude.



New default Quick Switcher hotkey:



\- Ctrl+Alt+M



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- src/MokoSnap.Core/

\- src/MokoSnap.Platform.Windows/

\- src/MokoSnap.Tests/

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not implement Chrome tab capture changes.

\- Do not change Chrome native messaging protocol.

\- Do not modify installer workflow unless absolutely necessary.

\- Do not refactor unrelated UI.

\- Do not change preset execution behavior except single-instance activation behavior.

\- Do not add third-party single-instance libraries.



\## Assumptions



\- MokoSnap should be a single-instance tray/background utility.

\- If MokoSnap is already running and the user launches it again, the existing instance should be shown/focused.

\- The second process should exit cleanly.

\- Global Quick Switcher default should be Ctrl+Alt+M.

\- Ctrl+Alt+Space should no longer be the default.

\- If a user explicitly customized their Quick Switcher hotkey, do not overwrite it.

\- If the stored Quick Switcher hotkey is missing, empty, or the old default Ctrl+Alt+Space, migrate it to Ctrl+Alt+M.

\- Per-preset hotkeys are separate and should not be changed.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



\### 1. Single Instance Guard



Implement a single-instance mechanism.



Requirements:



1\. Use a named mutex or equivalent Windows-safe mechanism.

2\. Only one MokoSnap instance should keep running.

3\. If a second instance starts:

&#x20;  - detect existing instance

&#x20;  - signal existing instance to show/focus the main window

&#x20;  - exit second instance cleanly

4\. Existing instance behavior:

&#x20;  - if hidden to tray, show main window

&#x20;  - if minimized, restore

&#x20;  - bring to foreground

&#x20;  - focus main window

5\. Must work when the first instance is running hidden in tray.

6\. Must not create duplicate tray icons.

7\. Must not register duplicate global hotkeys.

8\. Must release the mutex on clean exit.

9\. Avoid crashing if IPC/signaling fails; in that case, second instance should still exit with a clear fallback if possible.



Suggested simple implementation:



\- Named Mutex for ownership.

\- Named EventWaitHandle, named pipe, or lightweight local IPC for activation signal.

\- Existing instance listens for activation signal and dispatches show/focus on UI thread.



Keep it simple.



\### 2. Command Line Behavior



Support command line behavior if relevant:



1\. Normal launch:

&#x20;  - if first instance, start normally.

&#x20;  - if second instance, focus existing instance then exit.

2\. Launch with `--minimized`:

&#x20;  - if first instance, start hidden/minimized to tray.

&#x20;  - if second instance, do not force showing main window unless this is currently simpler and documented.

3\. Launch from Start Menu / Desktop shortcut while already running:

&#x20;  - focus existing main window.



\### 3. Quick Switcher Default Hotkey Change



Change the default Quick Switcher hotkey from:



\- Ctrl+Alt+Space



to:



\- Ctrl+Alt+M



Requirements:



1\. New installs/default settings should use Ctrl+Alt+M.

2\. Existing settings with missing/empty Quick Switcher hotkey should use Ctrl+Alt+M.

3\. Existing settings with old default Ctrl+Alt+Space should migrate to Ctrl+Alt+M.

4\. Existing settings with custom user value should be preserved.

5\. UI text/help/onboarding/docs should not mention Ctrl+Alt+Space as the default anymore.

6\. Command palette should still open reliably with the new hotkey.

7\. If Ctrl+Alt+M registration fails, show a clear warning and keep the app running.



\### 4. Hotkey Conflict Handling



Requirements:



1\. Validate that Quick Switcher hotkey does not conflict with per-preset hotkeys.

2\. If conflict exists:

&#x20;  - show clear warning

&#x20;  - do not crash

&#x20;  - do not register duplicate hotkeys

3\. Existing per-preset duplicate detection must still work.

4\. Ctrl+Alt+M should display as Ctrl+Alt+M.

5\. Ctrl+Alt+Space should still be valid if the user manually chooses it later, but it should not be the default.



\### 5. Tests



Add tests for:



1\. Default Quick Switcher hotkey is Ctrl+Alt+M.

2\. Missing Quick Switcher hotkey resolves to Ctrl+Alt+M.

3\. Old default Ctrl+Alt+Space migrates to Ctrl+Alt+M.

4\. Custom Quick Switcher hotkey is preserved.

5\. Quick Switcher hotkey conflict with preset hotkey is detected if validation logic is testable.

6\. Single-instance logic should be unit-tested where possible through abstractions, but avoid fragile tests that depend on real OS process spawning unless already supported.



\## Verification



Run:



\- ./scripts/verify.ps1

\- ./scripts/publish-local.ps1



Manual check:



1\. Run MokoSnap.

2\. Confirm Quick Switcher opens with Ctrl+Alt+M.

3\. Confirm Ctrl+Alt+Space is no longer registered by default.

4\. Confirm Claude can use Ctrl+Alt+Space without MokoSnap stealing it.

5\. Hide MokoSnap to tray.

6\. Launch MokoSnap.exe again.

7\. Confirm no second tray icon appears.

8\. Confirm existing MokoSnap window is shown/focused.

9\. Launch MokoSnap.exe repeatedly.

10\. Confirm still only one instance exists.

11\. Assign Ctrl+Alt+M to a preset and confirm conflict is detected or handled clearly.

12\. Use tray Exit and confirm app exits cleanly.

13\. Launch MokoSnap again and confirm it starts normally.



\## Done Criteria



\- Code builds.

\- Tests pass.

\- Publish script works.

\- Only one MokoSnap instance can run.

\- Second launch focuses existing instance.

\- No duplicate tray icons.

\- No duplicate hotkey registration.

\- Default Quick Switcher hotkey is Ctrl+Alt+M.

\- Old Ctrl+Alt+Space default migrates to Ctrl+Alt+M.

\- Custom user hotkeys are preserved.

\- No unrelated files changed.

\- Final response lists changed files.

\- Final response lists verification result.

\- Final response lists publish result.

\- Final response lists known limitations.

