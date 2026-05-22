\# Task: First-run onboarding



\## Goal



Add a simple first-run onboarding experience for MokoSnap.



The app now has many features. New users should immediately understand:



\- MokoSnap runs in the tray/background.

\- Ctrl+Alt+M opens the Quick Switcher by default.

\- Presets can be created manually or by capturing current apps.

\- Settings can change hotkeys/startup/tray behavior.

\- Chrome tab capture requires Chrome Capture Setup.



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- src/MokoSnap.Core/

\- src/MokoSnap.Tests/

\- docs/SPEC.md only if needed

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not change Chrome capture protocol.

\- Do not change installer workflow.

\- Do not change preset execution behavior.

\- Do not refactor unrelated UI.

\- Do not add third-party UI frameworks.

\- Do not implement a full tutorial system.



\## Assumptions



\- Onboarding should appear only once by default.

\- User can dismiss onboarding.

\- Dismissed state should persist in AppSettings.

\- User can reopen onboarding/help later from Settings or main window if simple.

\- Onboarding should be lightweight, not a complex wizard.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



\### 1. Settings Model



Add setting if needed:



\- `hasSeenFirstRunOnboarding`



Requirements:



1\. Default value should be false.

2\. When onboarding is dismissed, set it to true.

3\. Persist the setting.

4\. Existing settings files should load safely with default false.



\### 2. Onboarding Dialog



Add a simple dialog or panel shown on first launch.



Content should explain:



1\. MokoSnap runs in the tray/background.

2\. Closing the main window hides to tray if enabled.

3\. Default Quick Switcher hotkey:

&#x20;  - Ctrl+Alt+M

4\. How to create presets:

&#x20;  - Add preset manually

&#x20;  - Capture Current Apps

5\. How to run presets:

&#x20;  - Run button

&#x20;  - preset hotkeys

&#x20;  - Quick Switcher

6\. Chrome tab capture:

&#x20;  - optional

&#x20;  - requires Chrome Capture Setup

7\. Settings:

&#x20;  - change hotkeys

&#x20;  - startup

&#x20;  - tray behavior



Buttons:



\- Get Started

\- Open Settings

\- Open Chrome Capture Setup

\- Do not show again



Keep UI simple.



\### 3. Trigger Behavior



Requirements:



1\. On normal first launch, show onboarding after MainWindow is ready.

2\. Do not show onboarding if app starts minimized to tray.

3\. Do not show onboarding if `hasSeenFirstRunOnboarding` is true.

4\. If user clicks Do not show again or Get Started, persist dismissal.

5\. If user opens Settings/Chrome Capture from onboarding, still mark onboarding as seen unless this feels wrong in existing flow.



\### 4. Reopen Help



Add one simple way to reopen onboarding/help:



\- Main window button/menu: `Help / Getting Started`

\- or Settings button: `Show Getting Started`



No need for a full help system.



\### 5. Tests



Add tests for:



1\. AppSettings default hasSeenFirstRunOnboarding false.

2\. JSON roundtrip preserves hasSeenFirstRunOnboarding.

3\. Existing settings JSON without the field loads safely if testable.

4\. Onboarding state update logic if implemented in a testable service.



Avoid WPF UI automation tests.



\## Verification



Run:



\- ./scripts/verify.ps1

\- ./scripts/publish-local.ps1



Manual check:



1\. Reset or delete settings file.

2\. Run MokoSnap normally.

3\. Confirm onboarding appears.

4\. Click Get Started.

5\. Restart MokoSnap.

6\. Confirm onboarding does not appear again.

7\. Reopen onboarding from Help / Getting Started.

8\. Test Open Settings from onboarding.

9\. Test Open Chrome Capture Setup from onboarding.

10\. Start app minimized if supported and confirm onboarding does not pop over tray-only launch.

11\. Confirm existing tray, hotkey, Settings, preset run, and Chrome import behavior still works.



\## Done Criteria



\- Code builds.

\- Tests pass.

\- Publish script works.

\- Onboarding appears on first normal launch.

\- Onboarding dismissed state persists.

\- Onboarding can be reopened manually.

\- Existing app behavior still works.

\- No unrelated files changed.

\- Final response lists changed files.

\- Final response lists verification result.

\- Final response lists publish result.

\- Final response lists known limitations.

