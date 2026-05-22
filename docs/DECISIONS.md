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


\## 007. Window Placement Memory

MokoSnap remembers window position, size, monitor, and show state for captured Application targets.

MokoSnap does not restore true Windows Snap Layout metadata. It restores the resulting rectangle/state after a window has been snapped or manually arranged.

Reason:

- Position/maximized state is accessible through Win32 APIs.
- Windows Snap Layout metadata is not exposed as a stable public restore API.
- Game fullscreen modes are app-internal and cannot be reliably restored externally.


## 008. Second Launch Activation

MokoSnap is a single-instance tray/background app.

If a second process starts, it signals the existing instance to show and focus the main window, then exits. This also applies when the second process is launched with `--minimized`.

Reason:

- Start Menu and Desktop launches should reliably bring MokoSnap back.
- A single behavior avoids duplicate tray icons and duplicate global hotkey registration.
- Startup `--minimized` still applies when it is the first process.
