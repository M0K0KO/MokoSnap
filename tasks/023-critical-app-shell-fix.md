\# Task: Critical app shell fix



\## Goal



Fix critical MokoSnap shell/lifecycle bugs before adding any new features.



Current known bugs:



1\. Multiple MokoSnap instances can still run.

2\. Ctrl+Alt+M Quick Switcher hotkey does not work.

3\. Settings UI is not clearly visible/reachable.

4\. Main UI has become messy and needs a minimal usable shell layout.



This task is a stabilization task. Do not add new product features.



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- src/MokoSnap.Core/

\- src/MokoSnap.Platform.Windows/

\- src/MokoSnap.Tests/

\- docs/DECISIONS.md only if needed

\- docs/RELEASE.md only if needed



Forbidden changes:



\- Do not add new features.

\- Do not change Chrome capture protocol.

\- Do not change installer workflow.

\- Do not implement new Chrome extension behavior.

\- Do not refactor unrelated platform code.

\- Do not redesign the entire app visually.

\- Do not add third-party UI frameworks.

\- Do not hide failures behind silent fallbacks.



\## Assumptions



\- MokoSnap is a Windows tray/background utility.

\- Only one MokoSnap instance should run per user session.

\- The Quick Switcher default hotkey is Ctrl+Alt+M.

\- Settings must be reachable from main window, tray menu, and command palette.

\- UI should be simple and usable, not pretty.

\- A stable boring UI is better than a broken fancy UI.



If an assumption is wrong, stop and report it.



\## Part 1. Fix single-instance behavior



Requirements:



1\. Use one constant app-wide single-instance ID.

2\. The single-instance ID must not depend on:

&#x20;  - executable path

&#x20;  - build configuration

&#x20;  - working directory

&#x20;  - publish/debug layout

3\. Recommended ID:

&#x20;  - MokoSnap.SingleInstance

4\. Use a named mutex for ownership.

5\. If this process does not own the mutex:

&#x20;  - signal the existing instance to show/focus main window

&#x20;  - exit the second process immediately

6\. The second process must not:

&#x20;  - create a tray icon

&#x20;  - register hotkeys

&#x20;  - load the full app shell

&#x20;  - open MainWindow

7\. The first instance must listen for activation requests.

8\. Activation request should:

&#x20;  - show main window if hidden

&#x20;  - restore if minimized

&#x20;  - bring to foreground

&#x20;  - focus main window

9\. Must work when first instance is hidden to tray.

10\. Must work for both:

&#x20;  - dotnet run development launch

&#x20;  - published exe launch

11\. Add clear diagnostic logging/status if activation signaling fails.



Implementation guidance:



\- Do the single-instance check as early as possible in App startup.

\- Do it before creating tray icon and before registering hotkeys.

\- Named Mutex plus named EventWaitHandle is acceptable.

\- Named pipe is acceptable if already simple.

\- Do not overengineer.



Manual failure condition:



\- Starting MokoSnap twice must never create two tray icons.



\## Part 2. Fix Quick Switcher hotkey



Requirements:



1\. Ctrl+Alt+M must open Quick Switcher by default.

2\. Ctrl+Alt+Space must not be registered by default.

3\. If settings are missing, empty, or contain old default Ctrl+Alt+Space:

&#x20;  - migrate to Ctrl+Alt+M

4\. Custom user hotkey must be preserved.

5\. Hotkey registration must not depend on visible MainWindow being foreground.

6\. Hotkeys must work when MainWindow is hidden to tray.

7\. If MainWindow HWND is unreliable, create a dedicated hidden hotkey message window or stable HwndSource.

8\. RegisterHotKey failures must be visible in app status/settings.

9\. Do not silently fail if Ctrl+Alt+M cannot register.

10\. When settings change hotkey:

&#x20;   - unregister old hotkey

&#x20;   - register new hotkey

&#x20;   - update diagnostics

11\. Existing per-preset hotkeys must still work.

12\. Quick Switcher must appear focused when opened by hotkey.



Tests:



\- default quick switcher hotkey is Ctrl+Alt+M

\- old Ctrl+Alt+Space migrates to Ctrl+Alt+M

\- Ctrl+Alt+M conflict with preset hotkey is detected

\- digit-row hotkeys still display as Ctrl+Alt+1, not Ctrl+Alt+D1



Manual failure condition:



\- Pressing Ctrl+Alt+M while MokoSnap is hidden to tray must open Quick Switcher.



\## Part 3. Make Settings clearly reachable



Requirements:



1\. Settings must be reachable from the main window.

2\. Settings must be reachable from tray context menu.

3\. Settings must be reachable from command palette by typing:

&#x20;  - settings

&#x20;  - config

&#x20;  - preferences

4\. Main window must have an obvious Settings button.

5\. Do not rely on a hidden tab that the user may not notice.

6\. Settings window/dialog must activate and receive focus when opened.

7\. If Settings fails to open, show a clear error.

8\. Settings should show:

&#x20;  - Quick Switcher hotkey

&#x20;  - startup toggle

&#x20;  - start minimized to tray

&#x20;  - minimize to tray

&#x20;  - Chrome Capture Setup shortcut/status



Manual failure condition:



\- User should be able to find Settings within 3 seconds after opening MokoSnap.



\## Part 4. Replace messy main UI with a boring usable shell



Goal:



Make the main window boring, obvious, and usable.



Do not attempt a pretty redesign. Make a simple stable layout.



Required layout:



1\. Left navigation/sidebar:

&#x20;  - Presets

&#x20;  - Settings

&#x20;  - Chrome Capture

&#x20;  - Help

2\. Main area changes based on selected section.

3\. Presets section:

&#x20;  - preset list

&#x20;  - selected preset details

&#x20;  - targets list

&#x20;  - obvious buttons:

&#x20;    - Add Preset

&#x20;    - Save

&#x20;    - Delete

&#x20;    - Run

&#x20;    - Capture Current Apps

&#x20;    - Import Chrome Tabs

4\. Settings section:

&#x20;  - show or embed settings UI

&#x20;  - or provide a large Open Settings button if embedding is too much

5\. Chrome Capture section:

&#x20;  - Chrome Capture Setup

&#x20;  - Import Latest Chrome Tabs

&#x20;  - latest capture status

6\. Help section:

&#x20;  - Getting Started

&#x20;  - hotkey summary

&#x20;  - short release/setup notes

7\. No important feature should be hidden in a tiny button.

8\. Avoid cramped layouts.

9\. Use simple WPF controls only.

10\. Do not introduce third-party UI libraries.

11\. Existing commands should keep working.



Important:



\- Do not rewrite business logic.

\- Only reorganize access points and layout.

\- Prefer simple panels and buttons.

\- Do not spend time on styling polish.



\## Part 5. Diagnostics panel



Add a small status/diagnostics area visible in the main window.



It should show at least:



1\. Quick Switcher hotkey status:

&#x20;  - registered

&#x20;  - failed

&#x20;  - disabled

2\. Single-instance status:

&#x20;  - active primary instance

3\. Tray status:

&#x20;  - running

4\. Chrome native host status:

&#x20;  - configured

&#x20;  - not configured

&#x20;  - warning/error

5\. Last operation status:

&#x20;  - preset run result

&#x20;  - settings save error

&#x20;  - hotkey registration error



This can be plain text. No fancy UI required.



\## Verification



Run:



\- ./scripts/verify.ps1

\- ./scripts/publish-local.ps1



Manual check:



1\. Run MokoSnap from development.

2\. Confirm main window has obvious navigation.

3\. Confirm Settings is visible/reachable.

4\. Hide MokoSnap to tray.

5\. Press Ctrl+Alt+M.

6\. Confirm Quick Switcher opens focused.

7\. Launch MokoSnap a second time.

8\. Confirm no second tray icon appears.

9\. Confirm existing instance is focused.

10\. Check Task Manager and confirm only one running MokoSnap app instance.

11\. Exit from tray.

12\. Launch published MokoSnap exe.

13\. Repeat steps 4 through 10 on published exe.

14\. Open Settings from:

&#x20;   - main window

&#x20;   - tray menu

&#x20;   - command palette

15\. Change Quick Switcher hotkey to Ctrl+Alt+Q.

16\. Confirm Ctrl+Alt+M stops working.

17\. Confirm Ctrl+Alt+Q works.

18\. Change it back to Ctrl+Alt+M.

19\. Confirm preset hotkeys still work.

20\. Confirm Capture Current Apps still works.

21\. Confirm Import Latest Chrome Tabs still works.

22\. Confirm no obvious WPF binding errors in debug output.



\## Done Criteria



\- Code builds.

\- Tests pass.

\- Publish script works.

\- Multiple instances are prevented.

\- Second launch focuses existing instance.

\- No duplicate tray icons.

\- Ctrl+Alt+M works by default.

\- Ctrl+Alt+Space is not used by default.

\- Settings is obvious and reachable.

\- Main UI is boring but usable.

\- Existing preset run, capture, close windows, tray, settings, and Chrome import still work.

\- Final response lists changed files.

\- Final response lists verification result.

\- Final response lists publish result.

\- Final response lists manual checks that still need user verification.

\- Final response lists known limitations.

