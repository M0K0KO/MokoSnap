\# Task: Chrome native host setup UI



\## Goal



Add MokoSnap UI support for setting up and diagnosing Chrome Native Messaging registration.



The current Chrome tab capture feature works, but setup is manual. MokoSnap should help the user generate the native host manifest, register the HKCU registry key, and diagnose common setup problems.



\## Scope



Allowed changes:



\- src/MokoSnap.Core/

\- src/MokoSnap.Platform.Windows/

\- src/MokoSnap.App/

\- src/MokoSnap.Tests/

\- extension/chrome/README.md

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not implement installer packaging.

\- Do not change Chrome tab capture protocol.

\- Do not parse Chrome internal session files.

\- Do not use Chrome DevTools Protocol.

\- Do not refactor unrelated UI.

\- Do not require admin rights.

\- Do not write to HKLM.

\- Do not add third-party registry/setup libraries.



\## Assumptions



\- Development setup uses the unpacked Chrome extension.

\- User can paste the Chrome extension ID from `chrome://extensions`.

\- Native host registration should use HKCU only.

\- Registry key path is `HKCU\\Software\\Google\\Chrome\\NativeMessagingHosts\\com.mokosnap.chrome\_capture`.

\- Registry default value should point to the generated native host manifest JSON file.

\- Generated manifest should be written under `%AppData%\\MokoSnap\\chrome-native-host-manifest.json`.

\- The native host executable path can be resolved from the running app/repo layout during development.

\- No admin permission should be required.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



\### 1. Setup Models



Add simple models if useful:



\- `ChromeNativeHostSetupStatus`

\- `ChromeNativeHostSetupRequest`

\- `ChromeNativeHostManifest`



Status should include:



\- extension ID present

\- extension origin valid

\- native host exe exists

\- manifest file exists

\- manifest JSON valid

\- registry key exists

\- registry value points to expected manifest

\- latest capture file exists

\- list of warnings/errors



\### 2. Windows Platform Service



Add a Windows service for Chrome native host setup.



Suggested name:



\- `ChromeNativeHostSetupService`



Responsibilities:



1\. Validate extension ID format.

2\. Build allowed origin:

&#x20;  - `chrome-extension://<extension-id>/`

3\. Locate `MokoSnap.NativeHost.exe`.

4\. Generate native host manifest JSON.

5\. Write manifest to `%AppData%\\MokoSnap\\chrome-native-host-manifest.json`.

6\. Register HKCU registry key:

&#x20;  - `Software\\Google\\Chrome\\NativeMessagingHosts\\com.mokosnap.chrome\_capture`

7\. Set default registry value to the manifest JSON path.

8\. Read current registry registration.

9\. Diagnose setup status.

10\. Do not require admin rights.

11\. Do not write to stdout from NativeHost setup logic.



\### 3. App UI



Add a simple setup section or dialog in MokoSnap.



UI requirements:



1\. Add button or menu item:

&#x20;  - `Chrome Capture Setup`

2\. Dialog should show:

&#x20;  - extension ID input box

&#x20;  - current native host exe path

&#x20;  - manifest output path

&#x20;  - registry status

&#x20;  - latest capture file status

&#x20;  - diagnostics messages

3\. Add buttons:

&#x20;  - `Check Status`

&#x20;  - `Register Native Host`

&#x20;  - `Open Chrome Extensions Page`

&#x20;  - `Open Extension Folder`

&#x20;  - `Open Latest Capture File` if it exists

4\. When user clicks `Register Native Host`:

&#x20;  - validate extension ID

&#x20;  - generate manifest

&#x20;  - write registry key

&#x20;  - re-check status

&#x20;  - show success/failure clearly

5\. If extension ID is invalid:

&#x20;  - show clear error

&#x20;  - do not write manifest/registry

6\. If native host exe is missing:

&#x20;  - show clear error suggesting to build `MokoSnap.NativeHost`

7\. Keep UI simple.

8\. Do not block existing Chrome import feature.



\### 4. README Update



Update `extension/chrome/README.md`.



It should explain both paths:



1\. Recommended:

&#x20;  - Use MokoSnap `Chrome Capture Setup` UI

2\. Manual fallback:

&#x20;  - Load unpacked extension

&#x20;  - Copy extension ID

&#x20;  - Create/register manifest manually



\### 5. Tests



Add tests for:



1\. Extension ID validation.

2\. Allowed origin generation.

3\. Manifest JSON generation.

4\. Setup status logic where possible.

5\. Registry behavior should be abstracted or faked in tests.

6\. Avoid tests that modify the real registry.



\## Verification



Run:



\- `./scripts/verify.ps1`



Manual check:



1\. Build solution.

2\. Run MokoSnap.

3\. Open `Chrome Capture Setup`.

4\. Paste the unpacked Chrome extension ID.

5\. Click `Check Status`.

6\. Click `Register Native Host`.

7\. Confirm `%AppData%\\MokoSnap\\chrome-native-host-manifest.json` exists.

8\. Confirm registry key exists under HKCU.

9\. Restart Chrome.

10\. Trigger extension capture.

11\. Confirm `%AppData%\\MokoSnap\\chrome-tabs-latest.json` is created.

12\. Click `Import Latest Chrome Tabs` in MokoSnap.

13\. Confirm tabs import correctly.

14\. Try invalid extension ID and confirm clear validation error.



\## Done Criteria



\- Code builds.

\- Tests pass.

\- MokoSnap can generate native host manifest.

\- MokoSnap can register HKCU native messaging host key.

\- MokoSnap can diagnose setup status.

\- Invalid extension IDs are rejected.

\- No admin rights required.

\- Existing Chrome tab import still works.

\- No unrelated files changed.

\- Final response lists changed files.

\- Final response lists verification result.

\- Final response lists known limitations.

