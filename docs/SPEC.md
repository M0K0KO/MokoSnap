\# MokoSnap SPEC



\## Purpose



MokoSnap switches the current Windows desktop work mode using presets.



Example presets:



\- Game Mode

\- Study Mode

\- Work Mode

\- Rendering Mode



\## Preset



A preset contains:



\- id

\- name

\- description

\- hotkey

\- closePolicy

\- closeConfirmationPolicy

\- targets\[]



\## Close Policy



\### None



Do not close existing windows.



\### CloseVisibleWindowsOnly



Close currently visible top-level application windows before launching preset targets.



Rules:



\- Ignore background processes.

\- Ignore invisible windows.

\- Ignore system windows.

\- Ignore MokoSnap.

\- Exclude Explorer by default.

\- Allow Explorer closing as an advanced option.

\- Try graceful close first.

\- Force close only after explicit confirmation.



\## Close Confirmation Policy



Each preset can choose:



\- AlwaysConfirm

\- SkipConfirmation



Default:



\- AlwaysConfirm



\## Target Types



\### ApplicationTarget



Fields:



\- displayName

\- executablePath

\- arguments

\- workingDirectory

\- launchDelayMs

\- runAsAdmin



\### ChromeTarget



Fields:



\- displayName

\- profileName

\- openInNewWindow

\- urls\[]



MVP:



\- Launch Chrome with URL list.



Later:



\- Capture all Chrome windows and tabs through Chrome Extension + Native Messaging.

\- Let the user select which captured tabs should be saved.



\### NotionTarget



Fields:



\- displayName

\- pageUrls\[]

\- preferDesktopApp



Behavior:



\- Try to open pages in Notion Desktop.

\- Fall back to shell open URL.



\### UrlTarget



Fields:



\- displayName

\- url



\### FolderTarget



Fields:



\- displayName

\- path



\## Hotkey UX



Required:



\- Per-preset global hotkey

\- Hotkey recorder UI

\- Conflict detection

\- Command palette global hotkey



Recommended default:



\- Ctrl + Alt + Space opens quick switcher

\- Search preset by typing

\- Enter runs selected preset

\- Esc closes switcher



\## Capture Current State



MokoSnap should support creating a preset from the current desktop state.



Capture scope:



\- Visible top-level app windows

\- Optional Explorer windows

\- Chrome tabs through Chrome Extension + Native Messaging



Not required:



\- Full app state restore

\- Window layout restore

\- Game/app internal state restore

