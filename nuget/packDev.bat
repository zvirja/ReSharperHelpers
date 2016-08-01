SET version="0.0.0.0"
IF NOT "%1"=="" SET version=%1

"c:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" ..\AlexPovar.ResharperTweaks.sln  /t:Build /p:Configuration=Debug
nuget pack ..\AlexPovar.ResharperTweaks.nuspec -BasePath ..\AlexPovar.ResharperTweaks\bin\Debug -Version %version%