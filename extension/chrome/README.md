# MokoSnap Chrome Capture Extension

Development setup:

1. Build MokoSnap.NativeHost:
   `dotnet build ../../src/MokoSnap.NativeHost/MokoSnap.NativeHost.csproj`
2. Open `chrome://extensions`.
3. Enable Developer mode.
4. Load unpacked extension from this `extension/chrome` directory.
5. Copy the extension ID from `chrome://extensions`.
6. Open MokoSnap.
7. Click `Chrome Capture Setup`.
8. Paste the extension ID.
9. Click `Check Status`.
10. Click `Register Native Host`.
11. Restart Chrome.

MokoSnap writes the development native host manifest to:
`%AppData%\MokoSnap\chrome-native-host-manifest.json`

MokoSnap registers the host under current user only:
`HKCU\Software\Google\Chrome\NativeMessagingHosts\com.mokosnap.chrome_capture`

Manual fallback:

1. Copy `native-host-manifest.template.json` to a local JSON file.
2. Replace `ABSOLUTE_PATH_TO_MokoSnap.NativeHost.exe` with the built native host path.
3. Replace `YOUR_EXTENSION_ID` with the Chrome extension ID.
4. Register the host manifest under current user:
   `HKCU\Software\Google\Chrome\NativeMessagingHosts\com.mokosnap.chrome_capture`
5. Set the registry key default value to the full path of the host manifest JSON.

The extension captures only the currently open Chrome tab metadata requested by MokoSnap:
window id, tab id, title, URL, active state, pinned state, and tab index.

After capture, the native host writes:
`%AppData%\MokoSnap\chrome-tabs-latest.json`

Published setup:

1. Run `./scripts/publish-local.ps1` from the repository root.
2. Launch `artifacts/publish/MokoSnap/MokoSnap.App/MokoSnap.App.exe`.
3. Open `Chrome Capture Setup`.
4. Paste the unpacked extension ID.
5. Click `Register Native Host`.
6. Restart Chrome.

When MokoSnap runs from the publish folder, the generated manifest points to the sibling published native host:
`artifacts/publish/MokoSnap/MokoSnap.NativeHost/MokoSnap.NativeHost.exe`
