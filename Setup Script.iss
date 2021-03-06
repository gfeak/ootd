#include "isxdl.iss"

#define MyAppName "Outlook on the Desktop"
#define MyAppVersion "3.4.0"
#define MyAppVerName "Outlook on the Desktop 3.4.0"
#define MyAppPublisher "Michael Scrivo"
#define MyAppURL "http://www.outlookonthedesktop.com"
#define MyAppExeName "OutlookDesktop.exe"
#define MyAppCopyright "�2006-2016 Michael Scrivo"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultGroupName={#MyAppName}
DefaultDirName={pf}\{#MyAppName}
VersionInfoCompany={#MyAppPublisher}
VersionInfoCopyright={#MyAppCopyright}
AppCopyright={#MyAppCopyright}
OutputBaseFilename=ootd-{#MyAppVersion}
Compression=lzma
SolidCompression=true
VersionInfoVersion={#MyAppVersion}
VersionInfoDescription={#MyAppName}
WizardImageFile=C:\Program Files (x86)\Inno Setup 5\WizModernImage-IS.bmp
WizardSmallImageFile=C:\Program Files (x86)\Inno Setup 5\WizModernSmallImage-IS.bmp
AppID={{6D9785D9-FF53-4C06-9C2A-E4173D41A2FD}
ShowLanguageDialog=yes
OutputDir=ServerStaging
MinVersion=0,6.0.6001sp2
AllowUNCPath=false
UninstallLogMode=append
UninstallDisplayIcon={app}\OutlookDesktop.exe
PrivilegesRequired=none
DisableDirPage=auto
DisableReadyMemo=True
DisableProgramGroupPage=yes
UsePreviousGroup=False

[Languages]
Name: eng; MessagesFile: compiler:Default.isl

[Run]
Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Flags: postinstall skipifsilent nowait runasoriginaluser unchecked; Description: "{cm:LaunchProgram,{#MyAppName}}"

[Tasks]
Name: installdotnet; Description: Download and Install Microsoft .NET Framework 4.6.1; Check: NeedsDotNetFramework

[Files]
Source: "OutlookDesktop\bin\x86\Release\OutlookDesktop.exe"; DestDir: {app}; Flags: ignoreversion
Source: "OutlookDesktop\bin\x86\Release\AxInterop.Microsoft.Office.Interop.OutlookViewCtl.dll"; DestDir: {app}; Flags: ignoreversion
Source: "OutlookDesktop\bin\x86\Release\OLXLib.dll"; DestDir: {app}; Flags: ignoreversion
Source: "OutlookDesktop\bin\x86\Release\OutlookDesktop.exe.config"; DestDir: {app}; Flags: ignoreversion
Source: "OutlookDesktop\bin\x86\Release\NetSparkle.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OutlookDesktop\bin\x86\Release\NLog.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "OutlookDesktop\bin\x86\Release\NLog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OutlookDesktop\bin\x86\Release\MACTrackBarLib.dll"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: {group}\{#MyAppName}; Filename: {app}\{#MyAppExeName}; WorkingDir: {app}
Name: {group}\{cm:ProgramOnTheWeb,{#MyAppName}}; Filename: {#MyAppURL}
Name: {group}\{cm:UninstallProgram,{#MyAppName}}; Filename: {uninstallexe}

[Registry]
Root: "HKCU"; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: OutlookOnDesktop; ValueData: {app}\OutlookDesktop.exe; Flags: uninsdeletevalue
Root: "HKCU"; Subkey: "Software\SMR Computer Services\Outlook On The Desktop"; Flags: createvalueifdoesntexist uninsdeletekey
Root: "HKCU"; Subkey: "Software\SMR Computer Services\Outlook On The Desktop\"; ValueType: string; ValueName: First Run; ValueData: True
Root: "HKCU"; Subkey: "Software\SMR Computer Services\Outlook On The Desktop\AutoUpdate"; Flags: createvalueifdoesntexist uninsdeletekey

[Dirs]
Name: "{app}\logs"; Permissions: everyone-modify

[Code]
const
	dotnetURL = 'http://download.microsoft.com/download/E/4/1/E4173890-A24A-4936-9FC9-AF930FE3FA40/NDP461-KB3102436-x86-x64-AllOS-ENU.exe';

function NeedsDotNetFramework(): Boolean;
var
	ReleaseVersion: Cardinal;
	tempResult: Boolean;
begin
	tempResult:= True;

	if RegKeyExists(HKLM,'Software\Microsoft\NET Framework Setup\NDP\v4\Full') then
	begin
		if RegQueryDWordValue(HKLM, 'Software\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', ReleaseVersion) then
		begin
      if (ReleaseVersion >= 394254) then
      begin
        tempResult := False;
      end;
		end;
	end;

	Result := tempResult;
end;

function NextButtonClick(CurPage: Integer): Boolean;
var
	hWnd: Integer;
	sFileName: String;
	sTasks: String;
	nCode: Integer;
begin
	Result := true;

	if CurPage = wpReady then begin
		hWnd := StrToInt(ExpandConstant('{wizardhwnd}'));

		sTasks := WizardSelectedTasks(false);

		isxdl_ClearFiles;
		isxdl_SetOption('title', 'Downloading the Microsoft .NET Framework 4.6.1 Framework');
		isxdl_SetOption('description', 'Please wait while Setup downloads the Microsoft .NET 4.6.1 Framework.');

		sFileName := ExpandConstant('{tmp}\NDP461-KB3102436-x86-x64-AllOS-ENU.exe');

		if IsTaskSelected('installdotnet') then
		begin
			isxdl_AddFile(dotnetURL, sFileName);
		end;

		if isxdl_DownloadFiles(hWnd) <> 0 then
		begin
			if FileExists(sFileName) then 
      begin 
        if Exec(sFileName,'/passive /norestart','',SW_SHOW,ewWaitUntilTerminated,nCode) then        
        begin 
          Result := true;
        end 
        else begin
          Result := false;
        end
      end 
		end 
    else begin
			Result := false;
    end
	end;
end;

[UninstallDelete]
Name: {app}\*; Type: filesandordirs
Name: {app}; Type: dirifempty

[InstallDelete]
Name: C:\{app}\*; Type: filesandordirs
Name: C:\{app}; Type: dirifempty
