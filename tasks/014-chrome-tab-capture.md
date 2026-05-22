\# Task: Chrome tab capture



\## Goal



Implement Chrome tab capture using a Chrome Extension plus Native Messaging.



The user should be able to capture currently open Chrome tabs and import selected tabs into the selected MokoSnap preset as a Chrome target.



\## Scope



Allowed changes:



\- src/MokoSnap.Core/

\- src/MokoSnap.Platform.Windows/

\- src/MokoSnap.App/

\- src/MokoSnap.Tests/

\- extension/chrome/

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not parse Chrome internal session files.

\- Do not use Chrome DevTools Protocol.

\- Do not use UI Automation to scrape Chrome address bars.

\- Do not implement installer packaging.

\- Do not refactor unrelated UI.

\- Do not add third-party browser automation libraries.



\## Assumptions



\- Chrome tab capture is implemented through Manifest V3 Chrome Extension.

\- The extension uses `chrome.tabs.query`.

\- Native Messaging is used to send captured tab data to a local MokoSnap native host.

\- MVP can use a simple native host that writes the latest capture to a JSON file under `%AppData%\\MokoSnap`.

\- MokoSnap.App can import the latest capture file through a UI button.

\- Full live two-way communication between MokoSnap.App and the extension can be deferred.

\- User will manually load the unpacked extension during development.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



\### 1. Chrome Extension



Create files under `extension/chrome/`:



\- `manifest.json`

\- `background.js`

\- `popup.html` if needed

\- `popup.js` if needed



Manifest requirements:



1\. Use Manifest V3.

2\. Request only necessary permissions:

&#x20;  - `tabs`

&#x20;  - `nativeMessaging`

3\. Add an action button if useful.

4\. When user triggers capture:

&#x20;  - query all Chrome windows/tabs

&#x20;  - collect:

&#x20;    - windowId

&#x20;    - tabId

&#x20;    - title

&#x20;    - url

&#x20;    - active

&#x20;    - pinned

&#x20;    - index

5\. Send captured data to native host using Native Messaging.

6\. Do not send unnecessary browsing data beyond currently captured tabs.



\### 2. Native Host



Add a small native host project if appropriate:



\- `src/MokoSnap.NativeHost/`



Requirements:



1\. Console application.

2\. Reads Native Messaging messages from stdin.

3\. Writes responses to stdout.

4\. Supports message type:

&#x20;  - `captureTabs`

5\. Writes latest capture payload to:

&#x20;  - `%AppData%\\MokoSnap\\chrome-tabs-latest.json`

6\. Return success/failure response to extension.

7\. Keep implementation minimal.

8\. Add project to solution.

9\. Update verify script if needed.



Native Messaging protocol requirements:



\- Messages are JSON.

\- Each message is prefixed by a 32-bit little-endian length.

\- Do not print logs to stdout because stdout is protocol data.

\- Write diagnostics to stderr or a log file if needed.



\### 3. Native Messaging Host Manifest



Add a development host manifest template:



\- `extension/chrome/native-host-manifest.template.json`



It should include:



\- host name, e.g. `com.mokosnap.chrome\_capture`

\- description

\- path placeholder to `MokoSnap.NativeHost.exe`

\- type `stdio`

\- allowed\_origins placeholder for the extension ID



Add documentation file:



\- `extension/chrome/README.md`



Include development setup steps:



1\. Build MokoSnap.NativeHost.

2\. Load unpacked Chrome extension.

3\. Get extension ID from `chrome://extensions`.

4\. Create host manifest from template.

5\. Register Windows registry key under current user:

&#x20;  - `HKCU\\Software\\Google\\Chrome\\NativeMessagingHosts\\com.mokosnap.chrome\_capture`

6\. Set the registry default value to the full path of the host manifest JSON.



\### 4. Core Models



Add simple models if needed:



\- `ChromeTabCapture`

\- `ChromeWindowCapture`

\- `ChromeTabInfo`



Fields:



\- capturedAt

\- windows\[]

\- tabs\[]



\### 5. MokoSnap App UI



Add UI support:



1\. Add button:

&#x20;  - `Import Latest Chrome Tabs`

2\. Button reads:

&#x20;  - `%AppData%\\MokoSnap\\chrome-tabs-latest.json`

3\. Show preview dialog:

&#x20;  - group by Chrome window

&#x20;  - list title + URL

&#x20;  - checkbox per tab

&#x20;  - select all / deselect all if simple

4\. On confirmation:

&#x20;  - create or append a Chrome target to the selected preset

&#x20;  - preserve selected tab order

&#x20;  - save preset

5\. If no capture file exists:

&#x20;  - show clear message explaining that the Chrome extension must capture first.

6\. If capture file is invalid:

&#x20;  - show clear error and do not crash.



\### 6. Tests



Add tests for:



1\. Native Messaging message length framing read/write if implemented in testable code.

2\. Chrome capture JSON deserialization.

3\. Selected tabs to Chrome target conversion.

4\. Invalid or missing capture file handling if app logic is testable.



Avoid tests that require real Chrome.



\## Verification



Run:



\- `./scripts/verify.ps1`



Manual check:



1\. Build solution.

2\. Load unpacked extension from `extension/chrome`.

3\. Register native host manifest in HKCU registry.

4\. Open several Chrome tabs across multiple windows.

5\. Trigger extension capture.

6\. Confirm `%AppData%\\MokoSnap\\chrome-tabs-latest.json` is created.

7\. Open MokoSnap.

8\. Select preset.

9\. Click `Import Latest Chrome Tabs`.

10\. Confirm tabs are shown grouped by window.

11\. Select a subset.

12\. Confirm selected tabs become a Chrome target.

13\. Save preset.

14\. Run preset.

15\. Confirm Chrome opens selected URLs.



\## Done Criteria



\- Code builds.

\- Tests pass.

\- Extension files exist.

\- Native host can receive and save capture payload.

\- MokoSnap can import latest captured tabs.

\- Selected tabs become Chrome target URLs.

\- No Chrome internal file parsing.

\- No DevTools Protocol.

\- No unrelated files changed.

\- Final response lists changed files.

\- Final response lists verification result.

\- Final response lists known limitations.

