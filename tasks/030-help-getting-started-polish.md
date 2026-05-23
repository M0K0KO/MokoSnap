\# Task: Help and Getting Started page polish



\## Goal



Make the Help / Getting Started navigation section useful and readable.



The Help page should explain the core MokoSnap workflow without opening documentation or requiring prior knowledge.



This is UI/content polish only. Do not change app behavior.



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- docs/README.md only if useful

\- docs/RELEASE.md only if useful

\- src/MokoSnap.Tests/ only if useful



Forbidden changes:



\- Do not change preset execution behavior.

\- Do not change Chrome capture protocol.

\- Do not change Native Messaging behavior.

\- Do not change tray, hotkey, single-instance, or startup behavior.

\- Do not change installer workflow.

\- Do not add new major features.

\- Do not add another UI framework.



\## Assumptions



\- MainWindow already has a Help navigation section.

\- First-run onboarding already exists.

\- Settings and Chrome Capture pages already exist.

\- The Help page should be concise and practical.



If an assumption is wrong, stop and report it.



\## Requirements



\### 1. Help Page Content



The Help page should explain:



\- What MokoSnap does

\- How to create a preset

\- How to capture current apps

\- How to run a preset

\- How Close Visible Windows works

\- How window placement restore works

\- How Quick Switcher works

\- How tray/background mode works

\- How Chrome tab capture works at a high level

\- Where to change settings



Keep the text short and useful.



\### 2. Quick Start Checklist



Add a visible quick start checklist:



1\. Create a preset

2\. Capture current apps

3\. Save preset

4\. Assign hotkey if desired

5\. Run preset

6\. Optional: set up Chrome Capture

7\. Optional: enable launch on startup



\### 3. Important Warnings



Add concise warnings:



\- Close Visible Windows sends graceful close requests to visible windows.

\- Unsaved work in other apps may prompt or block closing.

\- MokoSnap does not restore app-internal fullscreen/game state.

\- MokoSnap approximates Snap Layout by restoring window rectangle/state.

\- Chrome tab capture requires extension/native host setup.



\### 4. Action Buttons



Help page should expose useful buttons:



\- Open Getting Started

\- Open Settings

\- Open Chrome Capture Setup

\- Capture Current Apps

\- Open Quick Switcher



Use existing commands.



\### 5. Hotkey Summary



Show current Quick Switcher hotkey if available.



Also explain:



\- Default is Ctrl+Alt+M

\- Preset hotkeys can be set per preset

\- Hotkeys can be changed in Settings



\### 6. Behavior Preservation



The following must keep working:



\- Presets page

\- Settings page

\- Chrome Capture page

\- Quick Switcher

\- tray behavior

\- single-instance

\- preset run

\- capture current apps

\- Chrome import



\### 7. Tests



Add or update tests only if practical.



Avoid fragile WPF UI automation tests.



\## Verification



Run:



\- ./scripts/verify.ps1

\- ./scripts/publish-local.ps1



Manual check:



1\. Run MokoSnap.

2\. Open Help page.

3\. Confirm the page is readable and useful.

4\. Click Open Getting Started.

5\. Click Open Settings.

6\. Click Open Chrome Capture Setup.

7\. Click Open Quick Switcher.

8\. Confirm the current Quick Switcher hotkey is shown correctly.

9\. Confirm Presets, Settings, Chrome Capture pages still work.

10\. Confirm tray and Quick Switcher still work.



\## Done Criteria



\- Code builds.

\- Tests pass.

\- Publish script works.

\- Help page is useful.

\- Quick start checklist exists.

\- Warnings are clear.

\- Action buttons work.

\- Existing behavior remains unchanged.

\- No unrelated files changed.

\- Final response lists changed files.

\- Final response lists verification result.

\- Final response lists publish result.

\- Final response lists manual checks still needed.

\- Final response lists known limitations.

