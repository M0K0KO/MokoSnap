\# Task: Introduce Wpf.Ui shell



\## Goal



Introduce Wpf.Ui and replace the plain messy MainWindow shell with a simple Fluent-style navigation shell.



This task is UI shell cleanup only. It must not change MokoSnap behavior.



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- src/MokoSnap.App/MokoSnap.App.csproj

\- src/MokoSnap.Tests/ only if needed

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not change preset execution behavior.

\- Do not change tray behavior.

\- Do not change single-instance behavior.

\- Do not change hotkey registration behavior.

\- Do not change Chrome capture protocol.

\- Do not change installer workflow.

\- Do not rewrite business logic.

\- Do not migrate every dialog yet.

\- Do not add multiple UI frameworks.



\## Assumptions



\- MokoSnap remains a WPF app.

\- Wpf.Ui is used only for app shell/navigation styling in this task.

\- Existing ViewModels and commands should be reused.

\- Existing dialogs can remain normal WPF dialogs for now.

\- A boring stable Fluent shell is better than a large risky redesign.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



\### 1. Add Wpf.Ui



1\. Add Wpf.Ui NuGet package to MokoSnap.App.

2\. Add required theme/resource setup in App.xaml or equivalent.

3\. Keep dependency limited to MokoSnap.App.

4\. Do not add another UI framework.



\### 2. Main Window Shell



Replace the current MainWindow layout with a simple Wpf.Ui based shell.



Preferred structure:



\- FluentWindow or compatible Wpf.Ui window

\- NavigationView or similar sidebar navigation

\- Main content area



Navigation items:



\- Presets

\- Settings

\- Chrome Capture

\- Help



Requirements:



1\. Main navigation must be obvious.

2\. Settings must be visible without hunting.

3\. Chrome Capture must be visible without hunting.

4\. Help / Getting Started must be visible.

5\. Do not hide important actions in tiny buttons.

6\. Do not remove existing commands.



\### 3. Presets Section



Presets section must preserve existing functionality:



\- preset list

\- selected preset details

\- targets list/editor access

\- Add Preset

\- Save

\- Delete

\- Run

\- Capture Current Apps

\- Import Latest Chrome Tabs



Do not rewrite preset logic.



\### 4. Settings Section



Settings section should expose existing settings behavior.



Acceptable options:



1\. Embed existing settings controls into the Settings section if simple.

2\. Or show a clear large Open Settings button that opens the existing Settings dialog.



Requirements:



\- Quick Switcher hotkey settings remain reachable.

\- Startup/tray settings remain reachable.

\- Settings from tray and command palette still work.



\### 5. Chrome Capture Section



Chrome Capture section should expose:



\- Chrome Capture Setup

\- Import Latest Chrome Tabs

\- latest capture/native host status if already available



Do not change Chrome capture protocol.



\### 6. Help Section



Help section should expose:



\- Getting Started / onboarding reopen

\- default hotkey summary

\- short usage notes:

&#x20; - app runs in tray

&#x20; - Ctrl+Alt+M opens Quick Switcher by default

&#x20; - X hides to tray if enabled

&#x20; - Chrome capture requires setup



\### 7. Diagnostics Visibility



Keep or improve the current diagnostics panel.



It should still show:



\- Quick Switcher hotkey status

\- single-instance status

\- tray status

\- Chrome capture/native host status

\- last operation status



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



\### 9. Tests



Add or update tests only if needed.



Avoid fragile WPF UI automation tests.



If there are existing source-structure tests or binding guard tests, update them carefully.



\## Verification



Run:



\- ./scripts/verify.ps1

\- ./scripts/publish-local.ps1



Manual check:



1\. Run MokoSnap.

2\. Confirm new Wpf.Ui shell appears.

3\. Confirm left navigation has Presets, Settings, Chrome Capture, Help.

4\. Confirm Presets section still supports Add, Save, Delete, Run.

5\. Confirm Capture Current Apps still works.

6\. Confirm Import Latest Chrome Tabs still works.

7\. Confirm Settings is easy to find.

8\. Confirm Settings from tray still works.

9\. Confirm Settings from command palette still works.

10\. Hide MokoSnap to tray.

11\. Press Ctrl+Alt+M.

12\. Confirm only Quick Switcher appears.

13\. Launch second instance and confirm existing instance focuses.

14\. Confirm no duplicate tray icons.

15\. Confirm publish output launches.



\## Done Criteria



\- Code builds.

\- Tests pass.

\- Publish script works.

\- Wpf.Ui package is added.

\- MainWindow has a clear navigation shell.

\- Settings is obvious.

\- Chrome Capture is obvious.

\- Existing behavior still works.

\- No unrelated source files changed.

\- Final response lists changed files.

\- Final response lists verification result.

\- Final response lists publish result.

\- Final response lists manual checks still needed.

\- Final response lists known limitations.

