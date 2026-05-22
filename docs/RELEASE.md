# MokoSnap Release Notes

MokoSnap currently ships as a local publish folder and as a simple Inno Setup installer.

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

## Manual Smoke Checklist

Before release, run this checklist against the local publish output:

1. Run `./scripts/verify.ps1`.
2. Run `./scripts/publish-local.ps1`.
3. Launch the published app from `artifacts/publish/MokoSnap/MokoSnap.App/MokoSnap.App.exe`.
4. Check tray behavior: minimize, restore from tray, and exit from tray.
5. Check the Quick Switcher opens with `Ctrl+Alt+M`.
6. Change the Quick Switcher hotkey in Settings and confirm the new hotkey works.
7. Use `Capture Current Apps` and confirm captured targets are added.
8. Run a preset and confirm target results are shown.
9. Test `CloseVisibleWindowsOnly` with preview/cancel/confirm behavior.
10. Open Chrome Capture Setup, register the native host if needed, restart Chrome, capture tabs, and import latest Chrome tabs.
11. Reopen onboarding from `Help / Getting Started`.
12. Toggle startup registration and confirm the HKCU Run value is added/removed.

## Installer

Install Inno Setup 6 before building the installer:

```text
https://jrsoftware.org/isinfo.php
```

Build the installer:

```powershell
./scripts/build-installer.ps1
```

The installer build script runs `publish-local.ps1` before compiling the Inno Setup script.

Installer output:

```text
artifacts/installer/MokoSnapSetup.exe
```

The installer uses a per-user install location and does not require admin rights:

```text
%LocalAppData%\Programs\MokoSnap
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

When MokoSnap runs from the installed folder, Chrome Capture Setup resolves the installed sibling native host:

```text
%LocalAppData%\Programs\MokoSnap\MokoSnap.NativeHost\MokoSnap.NativeHost.exe
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

- The Chrome extension is still loaded manually as an unpacked extension.
- Chrome may need to be restarted after native host registration.
- The installer does not automatically register the Chrome Native Host.
- The installer does not install the Chrome extension.
- The publish output is framework-dependent and requires the .NET 8 Desktop Runtime.
- The installer is framework-dependent and requires the .NET 8 Desktop Runtime.
