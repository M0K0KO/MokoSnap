# MokoSnap Chrome Capture Extension

Development setup:

1. Build MokoSnap.NativeHost:
   `dotnet build ../../src/MokoSnap.NativeHost/MokoSnap.NativeHost.csproj`
2. Open `chrome://extensions`.
3. Enable Developer mode.
4. Load unpacked extension from this `extension/chrome` directory.
5. Copy the extension ID from `chrome://extensions`.
6. Copy `native-host-manifest.template.json` to a local JSON file.
7. Replace `ABSOLUTE_PATH_TO_MokoSnap.NativeHost.exe` with the built native host path.
8. Replace `YOUR_EXTENSION_ID` with the Chrome extension ID.
9. Register the host manifest under current user:
   `HKCU\Software\Google\Chrome\NativeMessagingHosts\com.mokosnap.chrome_capture`
10. Set the registry key default value to the full path of the host manifest JSON.

The extension captures only the currently open Chrome tab metadata requested by MokoSnap:
window id, tab id, title, URL, active state, pinned state, and tab index.

After capture, the native host writes:
`%AppData%\MokoSnap\chrome-tabs-latest.json`
