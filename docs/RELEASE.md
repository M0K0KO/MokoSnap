# MokoSnap Release Notes

MokoSnap currently ships as a local publish folder. There is no MSI, MSIX, or installer yet.

## Verify

Run the normal verification script before publishing:

```powershell
./scripts/verify.ps1
```

This restores packages, builds the solution in Debug, and runs the test suite.

## Local Publish

Create a Release publish folder:

```powershell
./scripts/publish-local.ps1
```

The script publishes framework-dependent outputs to:

```text
artifacts/publish/MokoSnap/
```

Expected executable paths:

```text
artifacts/publish/MokoSnap/MokoSnap.App/MokoSnap.App.exe
artifacts/publish/MokoSnap/MokoSnap.NativeHost/MokoSnap.NativeHost.exe
```

Run the published app by launching:

```powershell
./artifacts/publish/MokoSnap/MokoSnap.App/MokoSnap.App.exe
```

## Chrome Capture After Publish

1. Open `chrome://extensions`.
2. Enable Developer mode.
3. Load the unpacked extension from `extension/chrome`.
4. Copy the extension ID.
5. Run MokoSnap from `artifacts/publish/MokoSnap/MokoSnap.App/MokoSnap.App.exe`.
6. Open `Chrome Capture Setup`.
7. Paste the extension ID.
8. Click `Register Native Host`.
9. Restart Chrome.

When MokoSnap runs from the publish folder, Chrome Capture Setup resolves the sibling published native host:

```text
artifacts/publish/MokoSnap/MokoSnap.NativeHost/MokoSnap.NativeHost.exe
```

The native messaging registration is written under HKCU only:

```text
HKCU\Software\Google\Chrome\NativeMessagingHosts\com.mokosnap.chrome_capture
```

The generated manifest remains in:

```text
%AppData%\MokoSnap\chrome-native-host-manifest.json
```

## Startup

The startup toggle writes only to the current user's Run key:

```text
HKCU\Software\Microsoft\Windows\CurrentVersion\Run
```

The startup command uses the currently running `MokoSnap.App.exe`. When `Start minimized to tray` is enabled, the command includes `--minimized`.

## Known Limitations

- No installer yet.
- The Chrome extension is still loaded manually as an unpacked extension.
- Chrome may need to be restarted after native host registration.
- The publish output is framework-dependent and requires the .NET 8 Desktop Runtime.
