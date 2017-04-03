@echo off
SET version="0.0.0.0"
IF NOT "%1"=="" SET version=%1

for /f "usebackq tokens=1* delims=: " %%i in (`vswhere -latest -requires Microsoft.Component.MSBuild`) do (
  if /i "%%i"=="installationPath" set InstallDir=%%j
)

"%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" ..\code\AlexPovar.ReSharperHelpers.sln  /t:Build /p:Configuration=Debug
nuget pack ..\code\AlexPovar.ReSharperHelpers.nuspec -BasePath ..\code\AlexPovar.ReSharperHelpers\bin\Debug\net45 -Version %version%