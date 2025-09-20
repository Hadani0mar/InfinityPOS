[Setup]
AppName=SmartInventory Pro
AppVersion=1.5.1
AppPublisher=SmartInventory Pro Team
AppPublisherURL=https://github.com/Hadani0mar/InfinityPOS
AppSupportURL=https://github.com/Hadani0mar/InfinityPOS
AppUpdatesURL=https://github.com/Hadani0mar/InfinityPOS
DefaultDirName={autopf}\SmartInventory Pro
DefaultGroupName=SmartInventory Pro
AllowNoIcons=yes
LicenseFile=
OutputDir=dist
OutputBaseFilename=SmartInventoryPro_Setup_v1.5.1
SetupIconFile=
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "arabic"; MessagesFile: "compiler:Languages\Arabic.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1

[Files]
Source: "bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "appsettings.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "DOWNLOAD_FROM_GITHUB.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\SmartInventory Pro"; Filename: "{app}\SmartInventoryPro.exe"
Name: "{group}\{cm:UninstallProgram,SmartInventory Pro}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\SmartInventory Pro"; Filename: "{app}\SmartInventoryPro.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\SmartInventory Pro"; Filename: "{app}\SmartInventoryPro.exe"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\SmartInventoryPro.exe"; Description: "{cm:LaunchProgram,SmartInventory Pro}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
