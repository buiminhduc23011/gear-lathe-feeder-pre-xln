#define MyAppName "Gear Lathe Feeder Pre-XLN Desktop"
#define MyAppExeName "GearLatheFeeder.Desktop.exe"
#define RepoRoot AddBackslash(SourcePath) + ".."
#define DesktopProjectPath AddBackslash(RepoRoot) + "src\\Desktop.App\\Desktop.App.csproj"
#define BuildWorkingDir RepoRoot
#define BuildParams "publish """ + DesktopProjectPath + """ -c Release -f net10.0-windows --self-contained true -r win-x64"
#define BuildExitCode Exec("dotnet", BuildParams, BuildWorkingDir, 1)

; Auto-build Desktop.App in Release mode each time this installer script is compiled.
#if BuildExitCode != 0
  #error "dotnet publish failed with exit code " + Str(BuildExitCode)
#endif

#define MyAppSourceDir "..\src\Desktop.App\bin\Release\net10.0-windows\win-x64\publish"
#define MyAppExePath MyAppSourceDir + "\" + MyAppExeName
#ifndef MyAppVersion
  #define MyAppVersion GetVersionNumbersString(MyAppExePath)
#endif

#define MyAppIcon "..\src\Desktop.App\Resources\Images\Logo.ico"
#define DotNetDesktopRuntimeUrl "https://dotnet.microsoft.com/download/dotnet/10.0"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=Output
OutputBaseFilename={#MyAppName}_v{#MyAppVersion}_Setup
SetupIconFile={#MyAppIcon}
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional options:"

[Files]
Source: "{#MyAppSourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Code]
function HasDotNetDesktopRuntimeIn(const RuntimeRoot: string): Boolean;
var
  FindRec: TFindRec;
begin
  Result := False;

  if not DirExists(RuntimeRoot) then
  begin
    Exit;
  end;

  if FindFirst(AddBackslash(RuntimeRoot) + '10.*', FindRec) then
  begin
    try
      repeat
        if DirExists(AddBackslash(RuntimeRoot) + FindRec.Name) then
        begin
          Result := True;
          Exit;
        end;
      until not FindNext(FindRec);
    finally
      FindClose(FindRec);
    end;
  end;
end;

function IsDotNet10DesktopRuntimeInstalled: Boolean;
begin
  Result := HasDotNetDesktopRuntimeIn(ExpandConstant('{pf32}\dotnet\shared\Microsoft.WindowsDesktop.App'));

  if not Result and IsWin64 then
  begin
    Result := HasDotNetDesktopRuntimeIn(ExpandConstant('{pf64}\dotnet\shared\Microsoft.WindowsDesktop.App'));
  end;
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
end;
