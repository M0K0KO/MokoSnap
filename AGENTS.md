\# AGENTS.md



\## Project



MokoSnap is a Windows desktop mode switcher.



It lets users define presets that launch apps, Chrome tab groups, Notion pages, URLs, and folders. A preset can optionally close currently visible application windows before launching its targets.



\## Tech Stack



\- C# .NET 8

\- WPF

\- MVVM

\- xUnit

\- JSON local storage

\- Windows-specific APIs isolated under `MokoSnap.Platform.Windows`



\## Non-negotiable Rules



Follow these engineering rules:



1\. Think before coding.

2\. State assumptions before implementing.

3\. Prefer simple, minimal code.

4\. Do not add speculative abstractions.

5\. Make surgical changes only.

6\. Do not refactor unrelated code.

7\. Every task must define verification steps.

8\. Run verification before claiming done.



\## Architecture Rules



\- Do not put business logic in WPF code-behind.

\- Keep WPF views thin.

\- Put pure logic in `MokoSnap.Core`.

\- Put Win32-specific code in `MokoSnap.Platform.Windows`.

\- Use interfaces for platform-dependent services.

\- Tests should target Core logic first.

\- Platform code should be wrapped so it can be faked in tests.



\## Safety Rules



\- Never kill arbitrary background processes.

\- `CloseVisibleWindowsOnly` must only target top-level visible user windows.

\- MokoSnap must never close itself.

\- Explorer windows are optional close targets, but excluded by default.

\- Prefer graceful window close over force kill.

\- Force kill requires explicit user confirmation.



\## UX Rules



\- Hotkey UX is a first-class feature.

\- Provide a command-palette style quick switcher.

\- Preset execution must be possible without touching the mouse.

\- Preset editing must remain understandable.

\- Failed targets must be shown clearly.



\## Chrome Capture Rule



Do not parse Chrome internal session files.



Current Chrome tab capture should be implemented through a Chrome Extension plus Native Messaging.



The capture UX should list tabs from all Chrome windows and let the user select which tabs to save.



\## Notion Rule



Notion targets should prefer opening in the Notion desktop app, but must gracefully fall back to opening the page URL through the system shell if desktop open fails.



\## Verification



Before claiming done, run:



```powershell

./scripts/verify.ps1

