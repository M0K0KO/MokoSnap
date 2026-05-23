\# Task: Preset and target editor UX polish



\## Goal



Improve the usability of the Presets section.



The app shell is now clearer, but preset and target editing is still likely cramped and hard to use. This task should make preset creation, target editing, capture, and run flows easier to understand.



This is UI/UX polish only. Do not change core preset execution behavior.



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- src/MokoSnap.Tests/ only if useful

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not change preset execution logic.

\- Do not change visible window closing behavior.

\- Do not change hotkey registration behavior.

\- Do not change Chrome capture protocol.

\- Do not change installer workflow.

\- Do not refactor unrelated architecture.

\- Do not add another UI framework.

\- Do not rewrite storage/model logic unless a tiny display-only adjustment is required.



\## Assumptions



\- Wpf.Ui is already installed.

\- MainWindow already has a navigation shell.

\- Existing preset and target commands should be reused.

\- The priority is clarity and fewer cramped controls.

\- It is acceptable to keep existing dialogs if replacing them is risky.



If an assumption is wrong, stop and report it.



\## Requirements



\### 1. Presets section layout



Make the Presets section easier to use.



Preferred layout:



\- Left: preset list

\- Right top: selected preset summary/editor

\- Right middle: target list

\- Right bottom or toolbar: major actions



Required visible actions:



\- Add Preset

\- Save

\- Delete

\- Run

\- Capture Current Apps

\- Import Chrome Tabs



Requirements:



1\. Buttons should be clearly labeled.

2\. Dangerous actions like Delete should remain clearly separated.

3\. Run should be visually obvious.

4\. Capture Current Apps and Import Chrome Tabs should be easy to find.

5\. Avoid tiny hidden buttons for important actions.

6\. Do not remove existing functionality.



\### 2. Selected preset editor



Improve readability of selected preset fields.



Fields should be grouped:



\- Name

\- Description

\- Close policy

\- Close confirmation policy

\- Preset hotkey



Requirements:



1\. Labels should be clear.

2\. Hotkey display should show Ctrl+Alt+1, not Ctrl+Alt+D1.

3\. CloseVisibleWindowsOnly should include short warning/help text.

4\. If no preset is selected, show helpful empty state text.



\### 3. Target list readability



Improve target list display.



Each target row/card should show:



\- target type

\- display name

\- main path/url summary

\- launch delay if nonzero

\- window placement enabled if applicable



Requirements:



1\. Target type should be obvious.

2\. Chrome target should show number of URLs.

3\. Notion target should show number of pages.

4\. Application target should show executable path summary.

5\. Window placement indicator should be readable.

6\. Target order controls should remain available.

7\. Add/Edit/Delete target should remain available if currently supported.



\### 4. Empty states



Add useful empty states:



1\. No presets:

&#x20;  - show text explaining to add a preset or capture current apps.

2\. Preset selected but no targets:

&#x20;  - suggest Capture Current Apps, Import Chrome Tabs, or Add Target.

3\. Chrome capture unavailable:

&#x20;  - point to Chrome Capture Setup.



\### 5. Operation feedback



Improve feedback after common actions:



\- preset saved

\- preset deleted

\- target added/removed

\- current apps captured

\- Chrome tabs imported

\- preset run completed/failed



Requirements:



1\. Feedback can be simple status text.

2\. Do not spam modal MessageBoxes for successful operations.

3\. Errors must remain visible.

4\. Keep last operation status updated.



\### 6. Behavior preservation



The following must keep working:



1\. Add/Edit/Delete presets.

2\. Save/load presets.

3\. Add/Edit/Delete/reorder targets if currently supported.

4\. Capture Current Apps.

5\. Import Latest Chrome Tabs.

6\. Run Preset.

7\. CloseVisibleWindowsOnly preview/cancel/confirm.

8\. Window placement memory.

9\. Quick Switcher and preset hotkeys.

10\. Tray behavior.



\### 7. Tests



Add or update tests only if practical.



Avoid fragile WPF UI automation tests.



\## Verification



Run:



\- ./scripts/verify.ps1

\- ./scripts/publish-local.ps1



Manual check:



1\. Run MokoSnap.

2\. Open Presets section.

3\. Confirm the layout is readable.

4\. Create a preset.

5\. Edit name/description/hotkey/close policy.

6\. Capture Current Apps.

7\. Confirm captured targets are readable.

8\. Import Chrome tabs if capture file exists.

9\. Confirm Chrome target shows URL count.

10\. Reorder targets if supported.

11\. Save and restart app.

12\. Confirm preset and targets persisted.

13\. Run preset.

14\. Confirm operation feedback is visible.

15\. Delete preset.

16\. Confirm delete remains guarded and clear.

17\. Confirm Quick Switcher/tray/Settings still work.



\## Done Criteria



\- Code builds.

\- Tests pass.

\- Publish script works.

\- Presets section is easier to understand.

\- Target list is easier to read.

\- Empty states are helpful.

\- Operation feedback is clearer.

\- Existing behavior still works.

\- No unrelated files changed.

\- Final response lists changed files.

\- Final response lists verification result.

\- Final response lists publish result.

\- Final response lists manual checks still needed.

\- Final response lists known limitations.

