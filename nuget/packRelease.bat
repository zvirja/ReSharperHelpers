@echo off

for /f "usebackq tokens=1* delims=: " %%i in (`vswhere -latest -requires Microsoft.Component.MSBuild`) do (
  if /i "%%i"=="installationPath" set InstallDir=%%j
)

"%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" ..\code\AlexPovar.ReSharperHelpers.sln  /t:Build /p:Configuration=Release
nuget pack ..\code\AlexPovar.ReSharperHelpers.nuspec -BasePath ..\code\AlexPovar.ReSharperHelpers\bin\Release\net45