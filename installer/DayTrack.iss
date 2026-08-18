#define MyAppName "DayTrack"
#define MyAppVersion "1.0.0"
#define MyAppExeName "DayTrack.exe"
#define MyAppPublisher "DayTrack"

[Setup]
AppId={{8CFC98CD-A22A-4E85-A908-7D657FE13AA8}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\DayTrack
DefaultGroupName=DayTrack
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\dist
OutputBaseFilename=DayTrack-Setup-v1.0.0-win-x64
SetupIconFile=..\src\DayTrack.App\Assets\day_tracker_icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
VersionInfoVersion=1.0.0.0
VersionInfoProductName=DayTrack
VersionInfoDescription=Private local Windows activity tracker
VersionInfoProductVersion=1.0.0
VersionInfoTextVersion=1.0.0

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "..\release\DayTrack-win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\DayTrack"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{autodesktop}\DayTrack"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch DayTrack"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
function InitializeUninstall(): Boolean;
begin
  Result := True;
end;
