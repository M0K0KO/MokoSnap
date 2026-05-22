\# Task: Create initial solution



\## Goal



Create the initial MokoSnap .NET solution structure.



\## Scope



Allowed changes:



\- src/

\- MokoSnap.sln

\- scripts/verify.ps1

\- docs/DECISIONS.md only if a decision is necessary



Forbidden changes:



\- Do not implement preset logic yet.

\- Do not implement hotkeys yet.

\- Do not implement window closing yet.

\- Do not implement Chrome capture yet.

\- Do not add speculative libraries.



\## Assumptions



\- The app targets Windows.

\- The UI project uses WPF.

\- The solution uses .NET 8.



If any assumption is wrong, stop and report it.



\## Implementation Requirements



1\. Create `MokoSnap.sln`.

2\. Create projects:

&#x20;  - `src/MokoSnap.App`

&#x20;  - `src/MokoSnap.Core`

&#x20;  - `src/MokoSnap.Platform.Windows`

&#x20;  - `src/MokoSnap.Tests`

3\. `MokoSnap.App` must be a WPF app.

4\. `MokoSnap.Core` must be a class library.

5\. `MokoSnap.Platform.Windows` must be a class library.

6\. `MokoSnap.Tests` must be an xUnit test project.

7\. Add references:

&#x20;  - App references Core and Platform.Windows

&#x20;  - Platform.Windows references Core

&#x20;  - Tests references Core

8\. Add a minimal MainWindow that launches.

9\. Add one trivial xUnit test.

10\. Update `scripts/verify.ps1` only if needed.



\## Verification



Run:



```powershell

./scripts/verify.ps1

