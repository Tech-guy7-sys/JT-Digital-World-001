#define MyAppName "Agent Jeff"
#define MyAppPublisher "J Tech Digital World"

#ifndef AppVersion
  #define AppVersion "0.1.0"
#endif
#ifndef NumericVersion
  #define NumericVersion "0.1.0.0"
#endif

#define MyAppExeName "AgentJeff.exe"

[Setup]
AppId={{E4C3C9AD-2E4B-4FC1-B2AB-8E4CA0F09132}
AppName={#MyAppName}
AppVersion={#AppVersion}
AppVerName={#MyAppName} {#AppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\JT Digital World\Agent Jeff
DefaultGroupName=JT Digital World\Agent Jeff
DisableProgramGroupPage=yes
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
Uninstallable=yes
OutputDir=..\artifacts
OutputBaseFilename=Agent-Jeff-Setup-{#AppVersion}-x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
VersionInfoCompany={#MyAppPublisher}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#NumericVersion}
VersionInfoVersion={#NumericVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "..\artifacts\AgentJeff-win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\JT Digital World\Agent Jeff"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\Agent Jeff"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Agent Jeff"; Flags: postinstall nowait skipifsilent
