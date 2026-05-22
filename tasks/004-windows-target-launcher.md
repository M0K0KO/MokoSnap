\# Task: Windows target launcher



\## Goal



Implement the Windows platform target launcher that can launch MokoSnap target types.



\## Scope



Allowed changes:



\- src/MokoSnap.Platform.Windows/

\- src/MokoSnap.Core/ only if a tiny interface/result adjustment is required

\- src/MokoSnap.Tests/

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not modify WPF UI.

\- Do not implement preset CRUD UI.

\- Do not implement hotkeys.

\- Do not implement visible window enumeration or closing.

\- Do not implement Chrome tab capture.

\- Do not implement Chrome Extension code.

\- Do not add speculative libraries.



\## Assumptions



\- Use the existing model names and shapes from MokoSnap.Core.

\- The launcher should implement the existing `ITargetLauncher` abstraction if possible.

\- If the current abstraction is insufficient, make the smallest necessary adjustment.

\- Real process launching should be isolated so command-building logic can be unit-tested.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



1\. Implement a Windows target launcher in `MokoSnap.Platform.Windows`.

2\. Support target types:

&#x20;  - Application

&#x20;  - Url

&#x20;  - Folder

&#x20;  - Chrome

&#x20;  - Notion

3\. Application target:

&#x20;  - Use executable path.

&#x20;  - Use arguments if provided.

&#x20;  - Use working directory if provided.

&#x20;  - Respect runAsAdmin if the model already supports it.

4\. Url target:

&#x20;  - Open through Windows shell.

5\. Folder target:

&#x20;  - Open with Explorer or shell execute.

6\. Chrome target:

&#x20;  - Launch Chrome with configured URL list.

&#x20;  - Support `openInNewWindow` if present.

&#x20;  - Support `profileName` if present.

&#x20;  - Chrome executable path may be auto-resolved from common install locations.

&#x20;  - If Chrome cannot be found, fall back to shell-opening URLs.

7\. Notion target:

&#x20;  - Prefer opening page URLs in Notion desktop when possible.

&#x20;  - Fall back to shell-opening the page URLs.

&#x20;  - Do not use Notion API.

8\. Add testable command-building helpers where appropriate.

9\. Add unit tests for command-building logic.

10\. Avoid tests that actually launch external applications.



\## Verification



Run:



```powershell

./scripts/verify.ps1

