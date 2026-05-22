\# Task: Target editor UI



\## Goal



Implement WPF UI for editing targets inside a preset.



This task only edits target configuration. It does not execute presets.



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- src/MokoSnap.Core/ only if a tiny model adjustment is required

\- src/MokoSnap.Tests/ only if useful

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not implement preset execution UI.

\- Do not implement real target launching from UI.

\- Do not implement global hotkey registration.

\- Do not implement visible window enumeration or closing.

\- Do not implement Chrome tab capture.

\- Do not implement tray or startup behavior.

\- Do not add third-party UI/MVVM libraries.



\## Assumptions



\- Target configs already exist in Core.

\- UI should remain simple and usable.

\- Target editing should persist through the existing preset save flow.

\- Target order matters.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



1\. Show selected preset's target list.

2\. Support adding a target.

3\. Support editing a target.

4\. Support deleting a target.

5\. Support moving a target up/down.

6\. Support target types:

&#x20;  - Application

&#x20;  - Url

&#x20;  - Folder

&#x20;  - Chrome

&#x20;  - Notion

7\. Application target fields:

&#x20;  - display name

&#x20;  - executable path

&#x20;  - arguments

&#x20;  - working directory

&#x20;  - launch delay ms

&#x20;  - run as admin if model supports it

8\. Url target fields:

&#x20;  - display name

&#x20;  - url

&#x20;  - launch delay ms

9\. Folder target fields:

&#x20;  - display name

&#x20;  - path

&#x20;  - launch delay ms

10\. Chrome target fields:

&#x20;  - display name

&#x20;  - profile name

&#x20;  - open in new window

&#x20;  - urls, one URL per line

&#x20;  - launch delay ms

11\. Notion target fields:

&#x20;  - display name

&#x20;  - page urls, one URL per line

&#x20;  - prefer desktop app

&#x20;  - launch delay ms

12\. Keep target editor simple:

&#x20;  - either inline editor or dialog is acceptable

&#x20;  - do not overengineer dynamic forms

13\. Saving a preset must save its targets.

14\. Reopening the app must restore targets.

15\. Existing tests must continue passing.



\## Verification



Run:



```powershell

./scripts/verify.ps1

