\# Task: Pre-release bug sweep and diagnostics



\## Goal



Perform a focused pre-release bug sweep for MokoSnap.



Do not add major new features. Improve reliability, diagnostics, and obvious UX issues before installer/manual release testing.



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- src/MokoSnap.Core/

\- src/MokoSnap.Platform.Windows/

\- src/MokoSnap.Tests/

\- docs/RELEASE.md

\- docs/INSTALLER.md

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not add major new features.

\- Do not change Chrome capture protocol.

\- Do not refactor unrelated UI.

\- Do not change installer workflow unless fixing a small bug.

\- Do not add third-party frameworks.

\- Do not rewrite existing architecture.



\## Assumptions



\- MokoSnap is close to MVP release.

\- The priority is reliability and clear diagnostics.

\- Existing behavior should remain stable.

\- Small UX fixes are allowed.

\- Manual GUI behavior is important and should be documented where automation is not practical.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



\### 1. Binding / UI Error Sweep



Check WPF bindings for obvious runtime issues.



Requirements:



1\. Fix broken binding names if found.

2\. Fix missing DataContext issues if found.

3\. Fix obvious focus or activation regressions if found.

4\. Do not redesign UI.

5\. Add simple guard tests if existing pattern supports it.



\### 2. Hotkey Diagnostics



Improve hotkey diagnostics if needed.



Requirements:



1\. If Quick Switcher hotkey registration fails, show a clear warning.

2\. If preset hotkey registration fails, show which preset failed.

3\. If duplicate hotkeys exist, show readable messages.

4\. Ensure Ctrl+Alt+M default remains.

5\. Ensure Ctrl+Alt+1 style display remains correct.



\### 3. Tray / Single Instance Diagnostics



Check tray/background behavior.



Requirements:



1\. Avoid duplicate tray icons.

2\. Ensure tray icon is disposed on Exit.

3\. Ensure second instance activation path has clear fallback behavior.

4\. Ensure close-to-tray/minimize-to-tray respects settings.



\### 4. Chrome Capture Diagnostics



Improve user-facing diagnostics if needed.



Requirements:



1\. Missing latest capture file message should be clear.

2\. Invalid capture JSON message should be clear.

3\. NativeHost setup dialog should explain Chrome restart may be required.

4\. NativeHost path missing error should suggest running publish/build.

5\. Do not change capture protocol.



\### 5. Settings / Storage Diagnostics



Requirements:



1\. Settings load failure should not crash the app.

2\. Preset load failure should not crash the app.

3\. Invalid JSON should produce a clear message or safe fallback.

4\. Saving settings/presets failure should show an error.

5\. Do not silently lose user data.



\### 6. Published App Smoke Check Support



Add or update docs for manual smoke test.



Update `docs/RELEASE.md` with a concise checklist:



1\. Run verify.

2\. Run publish.

3\. Launch published app.

4\. Check tray behavior.

5\. Check Quick Switcher.

6\. Check settings hotkey change.

7\. Check capture current apps.

8\. Check run preset.

9\. Check close visible windows.

10\. Check Chrome capture setup/import.

11\. Check onboarding.

12\. Check startup registration.



\### 7. Tests



Add tests only where practical:



1\. hotkey diagnostics/conflict messages

2\. storage fallback behavior

3\. Chrome capture invalid JSON handling

4\. settings defaults/migration



Avoid fragile WPF UI automation tests.



\## Verification



Run:



\- ./scripts/verify.ps1

\- ./scripts/publish-local.ps1



Manual check:



1\. Launch development app.

2\. Launch published app.

3\. Test tray hide/show/exit.

4\. Test second launch focuses existing instance.

5\. Test Ctrl+Alt+M Quick Switcher.

6\. Test Settings hotkey change.

7\. Test preset run.

8\. Test CloseVisibleWindowsOnly preview/cancel/confirm.

9\. Test Capture Current Apps.

10\. Test Chrome Capture Setup status.

11\. Test Import Latest Chrome Tabs.

12\. Test onboarding reopen.

13\. Confirm no duplicate tray icons.

14\. Confirm no obvious unhandled exception dialogs.



\## Done Criteria



\- Code builds.

\- Tests pass.

\- Publish script works.

\- Diagnostics are clearer for common failure cases.

\- No major behavior regressions.

\- Release smoke checklist is documented.

\- No unrelated files changed.

\- Final response lists changed files.

\- Final response lists verification result.

\- Final response lists publish result.

\- Final response lists known limitations.

