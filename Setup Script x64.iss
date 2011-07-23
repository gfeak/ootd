#include "isxdl.iss"

#define MyAppName "Outlook on the Desktop"
#define MyAppVersion "1.6.1"
#define MyAppVerName "Outlook on the Desktop 1.6.1"
#define MyAppPublisher "Michael Scrivo"
#define MyAppURL "http://www.outlookonthedesktop.com"
#define MyAppExeName "OutlookDesktop.exe"
#define MyAppCopyright "�2006-2011 Michael Scrivo"

[Setup]
ArchitecturesInstallIn64BitMode=x64
AppName={#MyAppName}
AppVerName={#MyAppVerName}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
VersionInfoCompany={#MyAppPublisher}
VersionInfoCopyright={#MyAppCopyright}
AppCopyright={#MyAppCopyright}
OutputBaseFilename=ootd-{#MyAppVersion}-x64
Compression=lzma
SolidCompression=true
VersionInfoVersion={#MyAppVersion}
VersionInfoDescription={#MyAppName}
WizardImageFile=C:\Program Files (x86)\Inno Setup 5\WizModernImage-IS.bmp
WizardSmallImageFile=C:\Program Files (x86)\Inno Setup 5\WizModernSmallImage-IS.bmp
AppID={{6D9785D9-FF53-4C06-9C2A-E4173D41A2FD}
ShowLanguageDialog=yes
OutputDir=./
MinVersion=0,5.0.2195
AllowUNCPath=false
UninstallLogMode=append
UninstallDisplayIcon={app}\App.ico
PrivilegesRequired=none
AppMutex=Local\OutlookDesktop.exe
UsePreviousAppDir=false

[Languages]
Name: eng; MessagesFile: compiler:Default.isl

[Tasks]
Name: installdotnet; Description: Download and Install Microsoft .NET Framework 3.5; Check: NeedsDotNetFramework

[Files]
Source: OutlookDesktop\bin\x64\Release\OutlookDesktop.exe; DestDir: {app}; Flags: ignoreversion
Source: OutlookDesktop\bin\x64\Release\AxInterop.Microsoft.Office.Interop.OutlookViewCtl.dll; DestDir: {app}; Flags: ignoreversion
Source: OutlookDesktop\bin\x64\Release\Microsoft.Office.Interop.Outlook.dll; DestDir: {app}; Flags: ignoreversion
Source: OutlookDesktop\bin\x64\Release\OutlookView.dll; DestDir: {app}; Flags: ignoreversion
Source: OutlookDesktop\bin\x64\Release\OutlookDesktop.exe.config; DestDir: {app}; Flags: ignoreversion
Source: OutlookDesktop\bin\x64\Release\BitFactory.Logging.dll; DestDir: {app}; Flags: ignoreversion

[Icons]
Name: {group}\{#MyAppName}; Filename: {app}\{#MyAppExeName}; WorkingDir: {app}
Name: {group}\{cm:ProgramOnTheWeb,{#MyAppName}}; Filename: {#MyAppURL}
Name: {group}\{cm:UninstallProgram,{#MyAppName}}; Filename: {uninstallexe}

[Run]
Filename: {app}\{#MyAppExeName}; Description: {cm:LaunchProgram,{#MyAppName}}; Flags: postinstall skipifsilent nowait runasoriginaluser; WorkingDir: {app}

[Registry]
Root: HKCU; Subkey: Software\Microsoft\Windows\CurrentVersion\Run; ValueType: string; ValueName: OutlookOnDesktop; ValueData: {app}\OutlookDesktop.exe; Flags: uninsdeletevalue
Root: HKCU; Subkey: Software\SMR Computer Services\Outlook On The Desktop; Flags: createvalueifdoesntexist uninsdeletekey
Root: HKCU; Subkey: Software\SMR Computer Services\Outlook On The Desktop\; ValueType: string; ValueName: First Run; ValueData: True

[Code]
const
	dotnetURL = 'http://www.microsoft.com/downloads/info.aspx?na=90&p=&SrcDisplayLang=en&SrcCategoryId=&SrcFamilyId=333325fd-ae52-4e35-b531-508d977d32a6&u=http%3a%2f%2fdownload.microsoft.com%2fdownload%2f7%2f0%2f3%2f703455ee-a747-4cc8-bd3e-98a615c3aedb%2fdotNetFx35setup.exe';

function NeedsDotNetFramework(): Boolean;
var
	Installed: Cardinal;
	tempResult: Boolean;
begin
	tempResult:= True;

	if RegKeyExists(HKLM,'Software\Microsoft\NET Framework Setup\NDP\v3.5') then
	begin
		if RegQueryDWordValue(HKLM, 'Software\Microsoft\NET Framework Setup\NDP\v3.5', 'Install', Installed) then
		begin
			tempResult := False;
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
		isxdl_SetOption('title', 'Downloading the Microsoft .NET Framework 3.5');
		isxdl_SetOption('description', 'Please wait while Setup downloads the Microsoft .NET Framework 3.5 to your computer.');

		sFileName := ExpandConstant('{tmp}\dotNetFx35Setup.exe');

		if IsTaskSelected('installdotnet') then
		begin
			isxdl_AddFile(dotnetURL, sFileName);
		end;

		if isxdl_DownloadFiles(hWnd) <> 0 then
		begin
			if FileExists(sFileName) then Exec(sFileName,'/qb','',SW_SHOW,ewWaitUntilTerminated,nCode)
		end else
			Result := false;

	end;
end;
[UninstallDelete]
Name: {app}\*; Type: filesandordirs
Name: {app}; Type: dirifempty
[InstallDelete]
Name: C:\{app}\*; Type: filesandordirs
Name: C:\{app}; Type: dirifempty