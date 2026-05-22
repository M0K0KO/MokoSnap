\# Task: Basic preset CRUD UI



\## Goal



Implement a basic WPF UI for creating, viewing, editing, and deleting presets.



This task only handles preset metadata. It does not implement target editing, preset execution, hotkeys, tray, or window closing.



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- src/MokoSnap.Core/ only if a tiny model/storage adjustment is required

\- src/MokoSnap.Tests/ only if useful

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not implement target editor UI.

\- Do not implement preset execution UI.

\- Do not implement global hotkey registration.

\- Do not implement visible window enumeration or closing.

\- Do not implement Chrome tab capture.

\- Do not implement tray or startup behavior.

\- Do not add speculative UI frameworks or third-party MVVM libraries.



\## Assumptions



\- WPF UI uses simple MVVM.

\- No third-party MVVM framework is required.

\- Presets are loaded from existing JSON storage.

\- Presets are saved after create/update/delete.

\- Hotkey is stored as text/data only; no global registration yet.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



1\. Create a usable MainWindow.

2\. Display a list of presets.

3\. Show selected preset details.

4\. Support adding a new preset.

5\. Support editing:

&#x20;  - name

&#x20;  - description

&#x20;  - hotkey text/gesture field, depending on existing model

&#x20;  - close policy

&#x20;  - close confirmation policy

6\. Support deleting a preset with confirmation.

7\. Persist changes using existing JSON storage.

8\. If no presets file exists, start with an empty list.

9\. Keep UI simple:

&#x20;  - left side: preset list

&#x20;  - right side: selected preset details

&#x20;  - buttons: Add, Save, Delete

10\. Use async loading/saving where appropriate, but do not overcomplicate.

11\. Do not put business logic in code-behind except minimal window initialization.

12\. Add minimal view model classes:

&#x20;  - MainViewModel

&#x20;  - PresetEditorViewModel or equivalent

&#x20;  - RelayCommand or equivalent

13\. The app should build and launch.

14\. Existing tests must continue passing.



\## Verification



Run:



```powershell

./scripts/verify.ps1

