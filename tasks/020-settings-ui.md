\# Task: Settings UI



\## Goal



Add a simple settings dialog for MokoSnap.



The user should be able to configure core app behavior without editing JSON manually.



Settings must include:



\- Quick Switcher hotkey

\- launch on startup

\- start minimized to tray

\- minimize to tray

\- Chrome Capture Setup shortcut/status

\- hotkey validation and conflict detection



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- src/MokoSnap.Core/

\- src/MokoSnap.Platform.Windows/

\- src/MokoSnap.Tests/

\- docs/DECISIONS.md only if needed

\- docs/SPEC.md only if needed



Forbidden changes:



\- Do not change Chrome capture protocol.

\- Do not implement new Chrome extension behavior.

\- Do not modify installer workflow.

\- Do not refactor unrelated UI.

\- Do not change preset execution behavior.

\- Do not change visible window closing behavior.

\- Do not add third-party UI frameworks.



\## Assumptions



\- Settings are stored in existing AppSettings.

\- Settings dialog can be simple.

\- Quick Switcher hotkey is global app setting, separate from per-preset hotkeys.

\- Default Quick Switcher hotkey is Ctrl+Alt+M.

\- Startup registration uses HKCU only.

\- No admin rights are required.

\- Saving settings should refresh global hotkey registrations.

\- Cancel should discard unsaved changes.

\- Invalid settings should not be saved.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



\### 1. Settings Dialog



Add a settings dialog accessible from:



\- Main window button or menu

\- Tray context menu if tray menu exists

\- Command palette command if easy



Suggested label:



\- Settings



Dialog should include sections:



1\. General

2\. Hotkeys

3\. Startup / Tray

4\. Chrome Capture



Keep UI simple. Do not over-design.



\### 2. General Settings



Show basic app behavior settings:



\- minimize to tray

\- start minimized to tray



Requirements:



1\. `minimizeToTray` controls whether minimizing/closing main window hides to tray.

2\. `startMinimizedToTray` controls whether app starts hidden when launched normally or with startup.

3\. Settings should save to existing settings storage.

4\. Changes should apply without app restart where reasonable.



\### 3. Startup Registration



Add startup toggle:



\- Launch MokoSnap when Windows starts



Requirements:



1\. Use HKCU startup registration only.

2\. No admin rights.

3\. When enabled, register current app executable path with minimized argument if supported.

4\. When disabled, remove startup registration.

5\. Show current startup registration status:

&#x20;  - enabled

&#x20;  - disabled

&#x20;  - registered path mismatch

&#x20;  - error

6\. If registered path mismatch is detected, allow user to fix by toggling/registering again.

7\. Do not write to HKLM.



\### 4. Quick Switcher Hotkey Editor



Add editor for Quick Switcher hotkey.



Requirements:



1\. Show current Quick Switcher hotkey.

2\. Allow recording a new hotkey by pressing a key combination.

3\. Allow clearing the hotkey if supported.

4\. Validate hotkey before saving.

5\. Display digit-row keys as:

&#x20;  - Ctrl+Alt+1

&#x20;  - not Ctrl+Alt+D1

6\. Preserve NumPad distinction if already supported.

7\. Reject invalid hotkeys:

&#x20;  - single non-modifier key

&#x20;  - Alt+F4

&#x20;  - duplicate with preset hotkey

&#x20;  - duplicate with another app hotkey if registration fails

8\. Ctrl+Alt+M remains default.

9\. Ctrl+Alt+Space is allowed if user manually chooses it, but not default.

10\. On save:

&#x20;   - update settings

&#x20;   - unregister old Quick Switcher hotkey

&#x20;   - register new Quick Switcher hotkey

&#x20;   - show warning if registration fails

11\. Existing per-preset hotkeys must keep working.



\### 5. Preset Hotkey Conflict Visibility



Settings dialog should show warning if:



\- Quick Switcher hotkey conflicts with a preset hotkey

\- any preset hotkey has duplicate conflict

\- any hotkey failed OS registration



This does not need to be fancy. A simple warning text area is enough.



\### 6. Chrome Capture Section



Add Chrome Capture setup shortcut/status.



Requirements:



1\. Show whether latest Chrome capture file exists.

2\. Show whether native host registration appears configured if existing service supports it.

3\. Add button:

&#x20;  - Open Chrome Capture Setup

4\. Add button if simple:

&#x20;  - Open Chrome Extensions Page

5\. Do not change Chrome capture protocol.

6\. Do not install Chrome extension automatically.



\### 7. Save / Cancel Behavior



Requirements:



1\. Settings dialog should use a copy of settings while editing.

2\. Save persists changes.

3\. Cancel discards changes.

4\. Invalid hotkey prevents save.

5\. Startup registration changes should happen on save.

6\. If startup registration fails, show clear error and do not pretend success.

7\. Closing dialog with X should behave like Cancel.



\### 8. Integration



Requirements:



1\. Main window can open Settings.

2\. Tray menu can open Settings if tray menu exists.

3\. Command palette can open Settings if command palette already supports fixed commands.

4\. After settings save:

&#x20;  - quick switcher hotkey should immediately use new value

&#x20;  - tray/minimize behavior should use new value

&#x20;  - startup registration status should refresh



\### 9. Tests



Add tests for:



1\. default settings values

2\. Quick Switcher hotkey validation

3\. Quick Switcher vs preset hotkey conflict

4\. startup command generation if changed

5\. settings copy/save behavior if testable

6\. digit-row hotkey display remains Ctrl+Alt+1

7\. Ctrl+Alt+M default remains correct



Avoid fragile WPF UI automation tests.



\## Verification



Run:



\- ./scripts/verify.ps1

\- ./scripts/publish-local.ps1



Manual check:



1\. Run MokoSnap.

2\. Open Settings from main window.

3\. Confirm current Quick Switcher hotkey shows Ctrl+Alt+M.

4\. Change Quick Switcher hotkey to Ctrl+Alt+Q.

5\. Save.

6\. Confirm Ctrl+Alt+M no longer opens Quick Switcher.

7\. Confirm Ctrl+Alt+Q opens Quick Switcher.

8\. Change Quick Switcher hotkey to Ctrl+Alt+1.

9\. Confirm it displays as Ctrl+Alt+1, not Ctrl+Alt+D1.

10\. Try setting Quick Switcher hotkey equal to a preset hotkey.

11\. Confirm conflict is shown and invalid save is blocked or clearly warned.

12\. Toggle minimize to tray.

13\. Confirm minimize/close behavior follows setting.

14\. Toggle launch on startup.

15\. Confirm HKCU startup registration is added/removed.

16\. Open Settings from tray menu if available.

17\. Open Settings from command palette if available.

18\. Confirm Chrome Capture Setup button opens the existing setup dialog.

19\. Restart app and confirm saved settings persisted.



\## Done Criteria



\- Code builds.

\- Tests pass.

\- Publish script works.

\- Settings dialog exists.

\- Quick Switcher hotkey can be changed.

\- Hotkey conflicts are detected.

\- Startup registration can be toggled.

\- Tray/minimize behavior can be configured.

\- Chrome Capture Setup is reachable from Settings.

\- Saved settings persist after restart.

\- Existing preset execution, tray, command palette, and Chrome import still work.

\- No unrelated files changed.

\- Final response lists changed files.

\- Final response lists verification result.

\- Final response lists publish result.

\- Final response lists known limitations.

