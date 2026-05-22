\# Task: Core models and JSON storage



\## Goal



Implement MokoSnap core data models and JSON storage.



\## Scope



Allowed changes:



\- src/MokoSnap.Core/

\- src/MokoSnap.Tests/

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not modify WPF UI.

\- Do not implement process launching.

\- Do not implement hotkeys.

\- Do not implement Win32 window enumeration.

\- Do not implement Chrome extension code.



\## Assumptions



\- JSON storage is local file based.

\- Data is stored under `%AppData%\\MokoSnap`.

\- Models should be simple and serializable.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



1\. Implement `Preset`.

2\. Implement `TargetConfig`.

3\. Implement target type enum:

&#x20;  - Application

&#x20;  - Chrome

&#x20;  - Notion

&#x20;  - Url

&#x20;  - Folder

4\. Implement `ClosePolicy`:

&#x20;  - None

&#x20;  - CloseVisibleWindowsOnly

5\. Implement `CloseConfirmationPolicy`:

&#x20;  - AlwaysConfirm

&#x20;  - SkipConfirmation

6\. Implement `HotkeyGesture`.

7\. Implement `AppSettings`.

8\. Implement `LaunchHistory`.

9\. Implement JSON storage abstraction.

10\. Implement file-based JSON storage.

11\. Add unit tests for serialization/deserialization.

12\. Add unit tests for reasonable model defaults.



\## Verification



Run:



```powershell

./scripts/verify.ps1

