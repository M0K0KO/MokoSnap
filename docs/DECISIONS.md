\# MokoSnap Decisions



\## 001. Tech Stack



Use C# .NET 8 WPF.



Reason:



\- MokoSnap is Windows-only.

\- WPF is enough for this tool.

\- Win32 APIs are easy to access from C#.

\- Avoid Electron/Tauri overhead for MVP.



\## 002. Close Behavior



MokoSnap closes visible top-level windows, not arbitrary background processes.



Reason:



\- User requested visible window apps only.

\- Safer than process killing.

\- Matches desktop mode switching behavior.



\## 003. Explorer Behavior



Explorer is excluded by default, but can be included as an option.



Reason:



\- Explorer is both file manager and shell-related process.

\- Closing all explorer.exe blindly can be dangerous/confusing.



\## 004. Chrome Capture



Chrome capture uses Chrome Extension + Native Messaging.



Reason:



\- Do not parse Chrome internal session files.

\- Do not use fragile UI Automation hacks.

\- DevTools Protocol is not required.



\## 005. Notion Behavior



Notion targets prefer desktop app opening and fall back to shell URL opening.



Reason:



\- Specific page open can be represented as a Notion page URL.

\- App-level deep control is unnecessary for MVP.



\## 006. Confirmation Policy



CloseVisibleWindowsOnly shows a preview confirmation by default.



A preset may opt into SkipConfirmation.



Reason:



\- Safe default.

\- Power users can make hotkey switching fast.

