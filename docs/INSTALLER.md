# MokoSnap Installer

MokoSnap uses Inno Setup for a simple per-user Windows installer.

## Requirements

- Inno Setup 6
- .NET 8 Desktop Runtime on the target machine

Install Inno Setup manually from:

```text
https://jrsoftware.org/isinfo.php
```

The build script looks for `ISCC.exe` on `PATH` and in the standard Inno Setup 6 install locations.

## Build

Run these commands from the repository root:

```powershell
./scripts/verify.ps1
./scripts/publish-local.ps1
./scripts/build-installer.ps1
```

`build-installer.ps1` runs `publish-local.ps1` before compiling the installer.

The installer output is:

```text
artifacts/installer/MokoSnapSetup.exe
```

## Install

Run `artifacts/installer/MokoSnapSetup.exe`.

The installer uses a per-user location by default:

```text
%LocalAppData%\Programs\MokoSnap
```

Installed layout:

```text
%LocalAppData%\Programs\MokoSnap\MokoSnap.App
%LocalAppData%\Programs\MokoSnap\MokoSnap.NativeHost
```

The installer creates a Start Menu shortcut and offers an optional Desktop shortcut. It does not require admin rights and does not write HKLM registry keys.

## Chrome Capture After Install

1. Open `chrome://extensions`.
2. Enable Developer mode.
3. Load the unpacked extension from `extension/chrome`.
4. Copy the extension ID.
5. Launch MokoSnap from the Start Menu.
6. Open `Chrome Capture Setup`.
7. Paste the extension ID.
8. Click `Register Native Host`.
9. Restart Chrome.

Native Messaging registration is handled by MokoSnap under HKCU:

```text
HKCU\Software\Google\Chrome\NativeMessagingHosts\com.mokosnap.chrome_capture
```

The generated manifest points to the installed native host:

```text
%LocalAppData%\Programs\MokoSnap\MokoSnap.NativeHost\MokoSnap.NativeHost.exe
```

## Known Limitations

- The Chrome extension is still loaded manually as an unpacked extension.
- Chrome may need to be restarted after native host registration.
- The installer does not automatically register the Chrome Native Host.
- The installer does not install the Chrome extension.
- The installer is framework-dependent and requires the .NET 8 Desktop Runtime.
