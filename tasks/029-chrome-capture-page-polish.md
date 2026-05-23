\# Task: Chrome Capture page polish



\## Goal



Make the Chrome Capture navigation section directly useful.



The user should be able to understand and operate the Chrome tab capture workflow from one place:



1\. Load Chrome extension manually.

2\. Paste extension ID.

3\. Register Native Host.

4\. Capture tabs from Chrome extension.

5\. Import latest captured tabs into selected preset.



This task is UI integration/polish only. Do not change the Chrome capture protocol.



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- src/MokoSnap.Tests/ only if useful

\- extension/chrome/README.md only if needed



Forbidden changes:



\- Do not change Chrome extension protocol.

\- Do not change Native Messaging message format.

\- Do not change installer workflow.

\- Do not change preset execution behavior.

\- Do not change hotkey/single-instance/tray behavior.

\- Do not add another UI framework.

\- Do not rewrite Chrome capture services.



\## Assumptions



\- Existing Chrome Capture Setup dialog/service works.

\- Existing Import Latest Chrome Tabs flow works.

\- MainWindow has a Chrome Capture navigation section.

\- It is acceptable to reuse existing setup dialog from the page.

\- It is acceptable to add direct setup controls only if simple.



If an assumption is wrong, stop and report it.



\## Requirements



\### 1. Chrome Capture Page Layout



Improve the Chrome Capture section layout.



It should clearly show:



\- Setup status

\- Latest capture status

\- Main action buttons

\- Short step-by-step instructions



Required visible actions:



\- Open Chrome Extensions Page

\- Open Extension Folder

\- Chrome Capture Setup

\- Import Latest Chrome Tabs

\- Refresh Status



\### 2. Status Display



Show readable status for:



\- Native Host registration:

&#x20; - configured

&#x20; - not configured

&#x20; - warning/error

\- Native Host exe path if available

\- latest capture file:

&#x20; - exists / missing

&#x20; - capturedAt if available

&#x20; - tab/window count if available

\- extension setup reminder



If existing diagnostics service already exposes this, reuse it.



\### 3. Setup Flow



Requirements:



1\. User should understand they must load the unpacked extension manually.

2\. User should understand they must paste extension ID into Chrome Capture Setup.

3\. User should understand Chrome may need restart after native host registration.

4\. Do not automate Chrome extension installation.

5\. Do not change Native Messaging protocol.



\### 4. Import Flow



Requirements:



1\. Import Latest Chrome Tabs button should be prominent.

2\. If no preset is selected, show clear message.

3\. If no capture file exists, show clear recovery guidance.

4\. If capture JSON is invalid, show clear error.

5\. If import succeeds, update last operation status.

6\. Imported Chrome target should still preserve selected tab order.



\### 5. Behavior Preservation



The following must keep working:



\- existing Chrome Capture Setup dialog

\- Import Latest Chrome Tabs

\- Preset save/load

\- Run Preset

\- Quick Switcher

\- tray behavior

\- Settings page

\- single-instance guard



\### 6. Tests



Add or update tests only if practical.



Avoid fragile WPF UI automation tests.



\## Verification



Run:



\- ./scripts/verify.ps1

\- ./scripts/publish-local.ps1



Manual check:



1\. Run MokoSnap.

2\. Open Chrome Capture section.

3\. Confirm setup steps are understandable.

4\. Confirm status displays native host/capture info.

5\. Click Open Chrome Extensions Page.

6\. Click Open Extension Folder.

7\. Click Chrome Capture Setup.

8\. Confirm setup dialog opens/focuses.

9\. Click Import Latest Chrome Tabs with no selected preset and confirm clear message.

10\. Select preset and import latest tabs if capture file exists.

11\. Confirm imported Chrome target appears.

12\. Confirm Presets/Settings/Quick Switcher/tray still work.



\## Done Criteria



\- Code builds.

\- Tests pass.

\- Publish script works.

\- Chrome Capture page is understandable.

\- Setup actions are easy to find.

\- Import action is easy to find.

\- Status messages are clear.

\- Existing Chrome capture behavior still works.

\- No unrelated files changed.

\- Final response lists changed files.

\- Final response lists verification result.

\- Final response lists publish result.

\- Final response lists manual checks still needed.

\- Final response lists known limitations.

