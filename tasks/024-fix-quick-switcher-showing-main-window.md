\# Task: Fix Quick Switcher showing main window



\## Goal



Fix a UX regression where opening the Quick Switcher also shows the main MokoSnap window.



Expected behavior:



\- Pressing the Quick Switcher hotkey should show only the Quick Switcher.

\- The main window should remain hidden/minimized if it was hidden to tray.

\- The main window should only be shown when the user explicitly chooses a command such as Settings, Presets, or Open MokoSnap.



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- src/MokoSnap.Platform.Windows/ only if a tiny activation helper adjustment is required

\- src/MokoSnap.Tests/ only if useful



Forbidden changes:



\- Do not change Chrome capture protocol.

\- Do not change installer workflow.

\- Do not refactor unrelated UI.

\- Do not change preset execution behavior.

\- Do not change single-instance behavior except if it is directly entangled with this bug.

\- Do not add third-party UI libraries.



\## Assumptions



\- MokoSnap is a tray/background utility.

\- Quick Switcher is an independent foreground popup.

\- Showing Quick Switcher must not imply showing the main settings window.

\- MainWindow activation is only needed for commands that explicitly open the main window.

\- Quick Switcher still needs reliable keyboard focus.



If an assumption is wrong, stop and report it.



\## Problem



Current behavior:



1\. Hide MokoSnap to tray.

2\. Press Ctrl+Alt+M.

3\. Quick Switcher appears.

4\. Main MokoSnap window also appears.



This is wrong.



Correct behavior:



1\. Hide MokoSnap to tray.

2\. Press Ctrl+Alt+M.

3\. Only Quick Switcher appears.

4\. Main window stays hidden.

5\. If user selects Settings or Presets command, then main window appears.



\## Requirements



\### 1. Quick Switcher Opening



1\. Opening Quick Switcher must not call:

&#x20;  - MainWindow.Show()

&#x20;  - MainWindow.Activate()

&#x20;  - ShowMainWindow()

&#x20;  - RestoreMainWindow()

&#x20;  unless the user explicitly selected a command that needs the main window.

2\. Quick Switcher should be able to show and focus itself independently.

3\. If owner is needed, avoid using MainWindow activation as a precondition.

4\. If setting Owner causes hidden main window to appear, remove Owner or use a safer owner strategy.

5\. Quick Switcher should appear centered and focused.

6\. Search box should receive keyboard focus immediately.

7\. Esc should close Quick Switcher.

8\. Enter should run selected command/preset.



\### 2. Commands That May Show Main Window



These commands are allowed to show the main window:



\- Settings

\- Presets

\- Config

\- Preferences

\- Open MokoSnap

\- Getting Started / Help if implemented as main window section



When such command is selected:



1\. Close Quick Switcher.

2\. Show/restore/focus MainWindow.

3\. Navigate to the requested section if supported.



\### 3. Preset Run From Quick Switcher



Running a preset from Quick Switcher should not show MainWindow unless required by the preset flow.



Expected behavior:



1\. Ctrl+Alt+M opens Quick Switcher only.

2\. Select preset.

3\. Quick Switcher closes.

4\. Preset runs.

5\. MainWindow stays hidden unless a close-window confirmation dialog or other required modal needs UI.



\### 4. CloseVisibleWindowsOnly Interaction



If selected preset requires CloseVisibleWindowsOnly with confirmation:



1\. Quick Switcher may close.

2\. Close windows confirmation dialog may appear.

3\. MainWindow should not appear just because confirmation dialog appears, unless WPF ownership makes it unavoidable.

4\. Prefer showing the confirmation dialog independently/focused.



\### 5. Single-instance Behavior Must Remain



Second app launch behavior is different from Quick Switcher behavior.



Requirements:



1\. Launching a second MokoSnap instance should still show/focus MainWindow.

2\. Pressing Quick Switcher hotkey should not show MainWindow.

3\. Do not break single-instance activation.



\### 6. Tests



Add tests where practical for command behavior separation:



1\. Quick Switcher open request does not request main window activation.

2\. Settings command requests main window activation.

3\. Preset run command does not request main window activation unless necessary.

4\. Single-instance activation path still requests main window activation if testable.



Avoid fragile WPF UI automation tests.



\## Verification



Run:



\- ./scripts/verify.ps1

\- ./scripts/publish-local.ps1



Manual check:



1\. Run MokoSnap.

2\. Hide it to tray.

3\. Press Ctrl+Alt+M.

4\. Confirm only Quick Switcher appears.

5\. Confirm MainWindow stays hidden.

6\. Type a preset name and press Enter.

7\. Confirm preset runs and MainWindow stays hidden.

8\. Hide to tray again.

9\. Press Ctrl+Alt+M.

10\. Type settings and press Enter.

11\. Confirm MainWindow appears and Settings is shown/reachable.

12\. Hide to tray again.

13\. Press Ctrl+Alt+M.

14\. Press Esc.

15\. Confirm MainWindow remains hidden.

16\. Launch second MokoSnap instance.

17\. Confirm existing MainWindow appears/focuses.

18\. Confirm no duplicate tray icon.



\## Done Criteria



\- Code builds.

\- Tests pass.

\- Publish script works.

\- Quick Switcher opens without showing MainWindow.

\- Settings/Presets commands still show MainWindow intentionally.

\- Preset run from Quick Switcher does not show MainWindow.

\- Single-instance second launch still shows MainWindow.

\- No unrelated files changed.

\- Final response lists changed files.

\- Final response lists verification result.

\- Final response lists publish result.

\- Final response lists known limitations.

