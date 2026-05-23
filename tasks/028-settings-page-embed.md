\# Task: Embed Settings page in Wpf.Ui shell



\## Goal



Make the Settings navigation section directly useful.



Currently Settings may still rely mainly on a separate dialog. This task should embed the main settings controls into the Wpf.Ui shell Settings section, while preserving existing Settings dialog access from tray and command palette if needed.



This is UI integration only. Do not change core settings behavior.



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- src/MokoSnap.Tests/ only if useful



Forbidden changes:



\- Do not change Chrome capture protocol.

\- Do not change installer workflow.

\- Do not change preset execution behavior.

\- Do not change tray behavior.

\- Do not change single-instance behavior.

\- Do not change hotkey registration logic except wiring existing save behavior.

\- Do not rewrite settings storage logic.

\- Do not add another UI framework.



\## Assumptions



\- Existing SettingsDialogViewModel or equivalent settings logic can be reused.

\- MainWindow Settings section should expose the same core settings.

\- Tray and command palette Settings behavior should continue working.

\- Save/Cancel behavior must remain clear.

\- Invalid settings should not be saved.



If an assumption is wrong, stop and report it.



\## Requirements



\### 1. Embedded Settings Section



Settings navigation section should show usable settings directly.



Required controls:



\- Quick Switcher hotkey

\- Launch on startup

\- Start minimized to tray

\- Minimize to tray

\- Chrome Capture Setup shortcut/status if already available



Required actions:



\- Save Settings

\- Cancel / Revert Changes

\- Open Chrome Capture Setup



\### 2. Reuse Existing Logic



Requirements:



1\. Reuse existing settings ViewModel/service logic where practical.

2\. Do not duplicate validation rules.

3\. Do not create a separate inconsistent settings code path.

4\. Quick Switcher hotkey validation must remain the same.

5\. Startup registration must still use HKCU only.

6\. Save must refresh hotkey registration if hotkey changed.

7\. Cancel must discard unsaved changes.



\### 3. Hotkey Editor Behavior



Requirements:



1\. Current hotkey should display as Ctrl+Alt+M by default.

2\. Digit row should display as Ctrl+Alt+1, not Ctrl+Alt+D1.

3\. Conflict with preset hotkey should be detected.

4\. Invalid hotkey should show clear warning.

5\. After save, new Quick Switcher hotkey should work immediately.



\### 4. Status and Feedback



Settings section should show:



\- save success

\- validation error

\- hotkey registration failure

\- startup registration status

\- startup path mismatch if existing logic detects it



Feedback can be simple status text.



\### 5. Preserve Dialog Access



If existing Settings dialog remains:



1\. Tray Settings should still work.

2\. Command palette settings/config/preferences should still work.

3\. It may open the same embedded logic in a dialog, or open/focus the main window Settings section.

4\. Do not leave duplicate inconsistent settings UI.



\### 6. Behavior Preservation



The following must keep working:



\- Ctrl+Alt+M Quick Switcher

\- Quick Switcher not showing MainWindow unless explicit command

\- single-instance second launch

\- tray hide/show/exit

\- preset run

\- Capture Current Apps

\- Import Latest Chrome Tabs

\- Chrome Capture Setup



\### 7. Tests



Add or update tests only if practical.



Avoid fragile WPF UI automation tests.



\## Verification



Run:



\- ./scripts/verify.ps1

\- ./scripts/publish-local.ps1



Manual check:



1\. Run MokoSnap.

2\. Open Settings navigation section.

3\. Confirm settings controls are visible directly.

4\. Change Quick Switcher hotkey to Ctrl+Alt+Q.

5\. Save.

6\. Confirm Ctrl+Alt+M stops working.

7\. Confirm Ctrl+Alt+Q works.

8\. Change back to Ctrl+Alt+M.

9\. Toggle minimize to tray and confirm behavior.

10\. Toggle startup registration and confirm HKCU registration.

11\. Open Settings from tray.

12\. Open Settings from command palette.

13\. Confirm there is no confusing duplicate settings state.

14\. Confirm preset run, tray, Quick Switcher, and Chrome import still work.



\## Done Criteria



\- Code builds.

\- Tests pass.

\- Publish script works.

\- Settings section is directly useful.

\- Settings save/cancel behavior works.

\- Quick Switcher hotkey can be changed from Settings section.

\- Existing tray/command palette Settings access still works.

\- No unrelated files changed.

\- Final response lists changed files.

\- Final response lists verification result.

\- Final response lists publish result.

\- Final response lists manual checks still needed.

\- Final response lists known limitations.

