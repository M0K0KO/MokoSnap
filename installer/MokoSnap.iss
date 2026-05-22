#define MyAppName "MokoSnap"
#define MyAppVersion "0.1.0"
#define MyAppPublisher "MokoSnap"
#define MyAppExeName "MokoSnap.App.exe"

[Setup]
AppId={{D8E63749-BF45-4E1A-887E-EE0C1B7F9F38}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\MokoSnap
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\artifacts\installer
OutputBaseFilename=MokoSnapSetup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\MokoSnap.App\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\artifacts\publish\MokoSnap\MokoSnap.App\*"; DestDir: "{app}\MokoSnap.App"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\artifacts\publish\MokoSnap\MokoSnap.NativeHost\*"; DestDir: "{app}\MokoSnap.NativeHost"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\MokoSnap"; Filename: "{app}\MokoSnap.App\{#MyAppExeName}"; WorkingDir: "{app}\MokoSnap.App"
Name: "{autodesktop}\MokoSnap"; Filename: "{app}\MokoSnap.App\{#MyAppExeName}"; WorkingDir: "{app}\MokoSnap.App"; Tasks: desktopicon

[Run]
Filename: "{app}\MokoSnap.App\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
