\# Task: Publish and packaging prep



\## Goal



Prepare MokoSnap for local release builds.



MokoSnap currently works in development, but several paths may depend on Debug build output or repository layout. This task should make local publish output usable as a self-contained app folder and make Chrome Native Host setup resolve the correct published NativeHost executable.



This task does not need to create a full installer.



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- src/MokoSnap.NativeHost/

\- src/MokoSnap.Core/

\- src/MokoSnap.Platform.Windows/

\- src/MokoSnap.Tests/

\- scripts/

\- docs/

\- extension/chrome/README.md

\- MokoSnap.sln or project files if needed



Forbidden changes:



\- Do not implement a full installer.

\- Do not require admin rights.

\- Do not write to HKLM.

\- Do not change Chrome capture protocol.

\- Do not refactor unrelated UI.

\- Do not add heavy packaging frameworks unless absolutely necessary.



\## Assumptions



\- Initial release can be a local publish folder, not an MSI/MSIX installer.

\- MokoSnap.App and MokoSnap.NativeHost should both be published.

\- Chrome Native Host manifest should point to the published MokoSnap.NativeHost executable when available.

\- HKCU registry registration remains the correct Native Messaging registration method.

\- Startup registration should point to the published MokoSnap.App executable when available.

\- Development mode should still work from `dotnet run`.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



\### 1. Publish Script



Add a script:



\- `scripts/publish-local.ps1`



The script should:



1\. Clean or create a local publish directory:

&#x20;  - `artifacts/publish/MokoSnap/`

2\. Publish `MokoSnap.App`.

3\. Publish `MokoSnap.NativeHost`.

4\. Place outputs in predictable subfolders or one app folder.

5\. Print final paths:

&#x20;  - MokoSnap app exe

&#x20;  - MokoSnap native host exe

&#x20;  - publish directory

6\. Use Release configuration.

7\. Prefer framework-dependent publish unless self-contained is already configured simply.

8\. Avoid requiring admin rights.



Example expected output paths:



\- `artifacts/publish/MokoSnap/MokoSnap.App/MokoSnap.App.exe`

\- `artifacts/publish/MokoSnap/MokoSnap.NativeHost/MokoSnap.NativeHost.exe`



Exact layout may differ if documented.



\### 2. Path Resolution



Update platform setup/path resolution logic so it can find NativeHost in both cases:



1\. Development layout:

&#x20;  - repo `src/MokoSnap.NativeHost/bin/...`

2\. Published layout:

&#x20;  - near the published app folder

&#x20;  - or a documented sibling folder



The Chrome Native Host Setup UI should show the resolved NativeHost path.



If multiple candidates exist, prefer:



1\. Explicit configured path if present

2\. Published sibling NativeHost exe

3\. Development Debug/Release NativeHost exe



\### 3. Chrome Native Host Setup



Update Chrome setup so that generated manifest points to the best resolved NativeHost executable.



Requirements:



1\. If running from publish folder, generated manifest must point to published NativeHost exe.

2\. If running from development, generated manifest may point to development NativeHost exe.

3\. If no NativeHost exe is found, show clear diagnostic.

4\. Do not silently write an invalid manifest.



\### 4. Startup Registration Check



If startup registration already exists:



1\. Ensure startup registration can use the current running app executable path.

2\. Ensure `--minimized` or equivalent argument is supported.

3\. If startup registration does not exist yet, add a minimal HKCU-only service and UI toggle if it fits existing settings UI.



If startup registration already works, do not rewrite it unnecessarily.



\### 5. Documentation



Add or update:



\- `docs/RELEASE.md`



It should explain:



1\. How to run verify.

2\. How to run local publish script.

3\. Where output appears.

4\. How to run MokoSnap from published folder.

5\. How to use Chrome Capture Setup after publish.

6\. Known limitations:

&#x20;  - no installer yet

&#x20;  - unpacked extension still manual

&#x20;  - Chrome may need restart after native host registration



Update `extension/chrome/README.md` if needed.



\### 6. Tests



Add tests for:



1\. NativeHost path candidate selection logic if implemented in Core/testable code.

2\. Manifest generation with published NativeHost path.

3\. Startup command generation if added or modified.



Avoid tests that depend on actual registry writes.



\## Verification



Run:



\- `./scripts/verify.ps1`

\- `./scripts/publish-local.ps1`



Manual check:



1\. Run verify.

2\. Run publish script.

3\. Launch MokoSnap from publish output.

4\. Open Chrome Capture Setup.

5\. Paste extension ID.

6\. Register Native Host.

7\. Confirm generated manifest points to published NativeHost exe, not Debug exe, when running published app.

8\. Restart Chrome.

9\. Trigger Chrome extension capture.

10\. Confirm capture JSON is created.

11\. Import tabs in MokoSnap.

12\. Confirm startup registration, if present, points to the published app exe.



\## Done Criteria



\- Code builds.

\- Tests pass.

\- Publish script works.

\- Published MokoSnap launches.

\- Published app can resolve published NativeHost.

\- Chrome Native Host manifest generation uses correct path.

\- Release docs exist.

\- No admin rights required.

\- No unrelated files changed.

\- Final response lists changed files.

\- Final response lists verification result.

\- Final response lists known limitations.

