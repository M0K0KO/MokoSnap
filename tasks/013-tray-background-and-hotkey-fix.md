\# Task: Tray background mode and hotkey digit fix



\## Goal



Make MokoSnap behave like a background resident utility instead of a settings window that is always visible.



Also fix hotkey handling for digit-row keys such as Ctrl+Alt+1, which currently appear as Ctrl+Alt+D1 and may fail validation or saving.



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- src/MokoSnap.Core/

\- src/MokoSnap.Platform.Windows/

\- src/MokoSnap.Tests/

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not implement Chrome tab capture.

\- Do not implement installer packaging.

\- Do not refactor unrelated UI.

\- Do not change preset run behavior except how it is triggered from tray/hotkeys.

\- Do not add third-party tray or hotkey libraries unless absolutely necessary.



\## Assumptions



\- MokoSnap should run in the background.

\- Closing the main window should hide it to tray by default, not exit.

\- Exiting should be explicit through a tray menu item.

\- The main settings window can be reopened from tray.

\- Quick Switcher remains the primary hotkey-driven UI.

\- Digit-row keys are represented as D0-D9 by WPF internally, but should display as 0-9 to users.

\- Ctrl+Alt+1 through Ctrl+Alt+9 should be valid preset hotkeys.

\- Numpad keys may display as NumPad0-NumPad9 and should be handled separately if supported.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



\### 1. Tray Resident Behavior



1\. Add a system tray icon.

2\. Minimize behavior:

&#x20;  - If minimize-to-tray is enabled, minimizing hides the main window.

3\. Close behavior:

&#x20;  - Clicking X hides the main window to tray by default.

&#x20;  - It must not shut down the app unless the user chooses Exit.

4\. Add tray context menu:

&#x20;  - Open MokoSnap

&#x20;  - Quick Switcher

&#x20;  - Run Preset submenu or simple preset list if easy

&#x20;  - Exit

5\. Double-clicking the tray icon should open the main MokoSnap window.

6\. Exit from tray should:

&#x20;  - unregister hotkeys

&#x20;  - dispose tray icon

&#x20;  - shut down app cleanly

7\. Ensure MokoSnap keeps running when the main window is hidden.

8\. Ensure global hotkeys still work while the main window is hidden.



\### 2. Startup / Initial Visibility



1\. Add app setting if not already present:

&#x20;  - launchOnStartup

&#x20;  - startMinimizedToTray

&#x20;  - minimizeToTray

2\. Default behavior for normal development launch may still show the main window.

3\. If app is launched with `--minimized` or setting `startMinimizedToTray = true`, start hidden in tray.

4\. Do not require the settings window to stay visible for hotkeys to work.



\### 3. Main Window Access



1\. Add a reliable way to show settings/main window:

&#x20;  - tray menu `Open MokoSnap`

&#x20;  - optional global hotkey such as Ctrl+Alt+M if existing hotkey system makes it simple

2\. When opening main window:

&#x20;  - show if hidden

&#x20;  - restore if minimized

&#x20;  - activate/focus it

&#x20;  - bring it to foreground using existing focus helper if needed



\### 4. Hotkey Digit Fix



1\. Fix hotkey display:

&#x20;  - `Ctrl+Alt+D1` should display as `Ctrl+Alt+1`

&#x20;  - `Ctrl+Alt+D2` should display as `Ctrl+Alt+2`

&#x20;  - etc.

2\. Fix hotkey parsing:

&#x20;  - User text `Ctrl+Alt+1` should parse to digit-row key D1 or equivalent internal representation.

&#x20;  - Existing stored value `D1` should remain valid.

3\. Fix hotkey validation:

&#x20;  - Ctrl+Alt+D1 / Ctrl+Alt+1 must be valid.

&#x20;  - Ctrl+Alt+D0 through Ctrl+Alt+D9 must be valid.

&#x20;  - Single digit key with no modifier should remain invalid.

4\. Fix hotkey saving:

&#x20;  - A preset hotkey using digit-row keys must persist and reload correctly.

5\. Fix hotkey registration:

&#x20;  - Digit-row hotkeys should register through RegisterHotKey using the correct virtual key code.

6\. Add tests for:

&#x20;  - parse `Ctrl+Alt+1`

&#x20;  - display `Ctrl+Alt+1` instead of `Ctrl+Alt+D1`

&#x20;  - serialize/deserialize digit hotkey

&#x20;  - validate Ctrl+Alt+1 as allowed

&#x20;  - reject plain `1` with no modifier

&#x20;  - duplicate detection still works with `Ctrl+Alt+1`



\### 5. Safety



1\. Do not create multiple tray icons.

2\. Avoid duplicate hotkey registration when hiding/showing main window.

3\. Avoid app shutdown when closing the main window unless Exit was explicitly chosen.

4\. Ensure resources are disposed on Exit.



\## Verification



Run:



```powershell

./scripts/verify.ps1

