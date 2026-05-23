\# Task: Wpf.Ui dialog and status polish



\## Goal



Polish MokoSnap's Wpf.Ui shell experience without changing core behavior.



The MainWindow shell now uses Wpf.Ui, but status messages, diagnostics, and dialogs still feel inconsistent. This task should make common feedback and access points clearer using simple Wpf.Ui-friendly UI patterns.



This is UI polish only. Do not add new product features.



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- src/MokoSnap.App/MokoSnap.App.csproj only if needed

\- src/MokoSnap.Tests/ only if useful

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not change preset execution behavior.

\- Do not change tray behavior.

\- Do not change single-instance behavior.

\- Do not change hotkey registration behavior.

\- Do not change Chrome capture protocol.

\- Do not change installer workflow.

\- Do not rewrite business logic.

\- Do not migrate every dialog if risky.

\- Do not add another UI framework.

\- Do not make large visual redesigns.



\## Assumptions



\- Wpf.Ui is already added to MokoSnap.App.

\- MainWindow already uses a Wpf.Ui shell/navigation layout.

\- Existing dialogs can remain normal WPF if migration is risky.

\- The priority is clear, boring, consistent UI.

\- Existing behavior must remain stable.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



\### 1. Status and Diagnostics Polish



Improve the visible status/diagnostics area in MainWindow.



It should clearly show:



\- Quick Switcher hotkey status

\- single-instance status

\- tray status

\- Chrome native host status

\- latest Chrome capture status

\- last operation status



Requirements:



1\. Use a simple readable layout.

2\. Prefer Wpf.Ui-friendly controls if already available.

3\. Do not hide diagnostics behind tiny text.

4\. Use clear labels:

&#x20;  - OK

&#x20;  - Warning

&#x20;  - Error

&#x20;  - Not configured

5\. If a status has an action, expose a nearby button:

&#x20;  - Open Settings

&#x20;  - Chrome Capture Setup

&#x20;  - Import Latest Chrome Tabs

6\. Do not change the underlying diagnostic logic unless fixing a small display bug.



\### 2. Replace MessageBox Overuse Where Safe



Identify obvious MessageBox usage in common flows and replace only low-risk ones with better in-app status feedback or existing dialog patterns.



Candidate flows:



\- successful settings save

\- failed settings save

\- missing Chrome capture file

\- invalid Chrome capture file

\- successful Chrome native host registration

\- hotkey registration warning



Requirements:



1\. Do not remove necessary confirmation dialogs.

2\. Dangerous actions should still require confirmation.

3\. Errors must remain visible.

4\. Do not silently swallow failures.

5\. If replacing a MessageBox is risky, leave it alone.



\### 3. Settings Access Polish



Ensure Settings is visually obvious and consistent.



Requirements:



1\. Settings navigation item must be visible.

2\. Settings section should either embed settings or show a large clear Open Settings button.

3\. Tray Settings still works.

4\. Command palette settings/config/preferences still works.

5\. Settings dialog should activate/focus reliably.

6\. Settings dialog should not appear behind MainWindow.



\### 4. Chrome Capture Section Polish



Improve Chrome Capture section clarity.



It should show:



\- Chrome Capture Setup button

\- Import Latest Chrome Tabs button

\- latest capture file status

\- native host registration status

\- short instruction text



Instruction text should be concise:



\- Load Chrome extension manually.

\- Paste extension ID in Chrome Capture Setup.

\- Register Native Host.

\- Restart Chrome if capture fails.

\- Use Import Latest Chrome Tabs after capture.



Do not change protocol or extension behavior.



\### 5. Help Section Polish



Improve Help section readability.



It should mention:



\- MokoSnap runs in tray/background.

\- Default Quick Switcher hotkey is Ctrl+Alt+M.

\- X hides to tray if minimize-to-tray is enabled.

\- Capture Current Apps creates Application targets.

\- Chrome tab capture requires setup.

\- Settings can change hotkeys/startup/tray behavior.



Add clear buttons if already wired:



\- Getting Started

\- Settings

\- Chrome Capture Setup



\### 6. Dialog Focus Regression Guard



Do not regress existing focus behavior.



Requirements:



1\. Quick Switcher must still open without showing MainWindow.

2\. Quick Switcher search box must receive focus.

3\. Settings dialog must focus when opened.

4\. Close Windows dialog must focus when opened.

5\. Chrome Tabs dialog must focus when opened.

6\. Onboarding dialog must focus when opened.



Only make small focus fixes if obviously needed.



\### 7. Basic Visual Consistency



Apply minor consistency improvements only:



\- consistent button labels

\- consistent spacing

\- less cramped grouping

\- clearer section headers

\- fewer tiny controls for important actions



Do not spend time on pixel-perfect design.



\### 8. Behavior Preservation



The following must keep working:



1\. Single-instance guard.

2\. Tray hide/show/exit.

3\. Ctrl+Alt+M Quick Switcher.

4\. Quick Switcher does not show MainWindow unless explicit command.

5\. Settings command opens Settings.

6\. Preset run.

7\. Capture Current Apps.

8\. CloseVisibleWindowsOnly preview/cancel/confirm.

9\. Import Latest Chrome Tabs.

10\. Chrome Capture Setup.

11\. Startup registration settings.

12\. First-run onboarding.



\### 9. Tests



Add or update tests only if practical.



Avoid fragile WPF UI automation tests.



If source-structure tests exist, update them carefully.



\## Verification



Run:



\- ./scripts/verify.ps1

\- ./scripts/publish-local.ps1



Manual check:



1\. Run MokoSnap.

2\. Confirm MainWindow looks less cramped and navigation is obvious.

3\. Confirm diagnostics are visible and readable.

4\. Confirm Settings is obvious.

5\. Confirm Chrome Capture section explains setup and has useful buttons.

6\. Confirm Help section explains basic usage.

7\. Hide MokoSnap to tray.

8\. Press Ctrl+Alt+M.

9\. Confirm only Quick Switcher appears and search box is focused.

10\. Open Settings from main window.

11\. Open Settings from tray.

12\. Open Settings from command palette.

13\. Confirm Settings focuses correctly.

14\. Test Import Latest Chrome Tabs with missing/invalid file if practical.

15\. Confirm errors are clear.

16\. Confirm preset run still works.

17\. Confirm CloseVisibleWindowsOnly still shows confirmation dialog.

18\. Confirm published exe launches.



\## Done Criteria



\- Code builds.

\- Tests pass.

\- Publish script works.

\- Status/diagnostics area is clearer.

\- Settings is obvious and reachable.

\- Chrome Capture section is understandable.

\- Help section is useful.

\- Existing behavior still works.

\- No unrelated source files changed.

\- Final response lists changed files.

\- Final response lists verification result.

\- Final response lists publish result.

\- Final response lists manual checks still needed.

\- Final response lists known limitations.

