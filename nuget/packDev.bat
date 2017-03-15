SET version="0.0.0.0"
IF NOT "%1"=="" SET version=%1

"c:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe" ..\code\AlexPovar.ReSharperHelpers.sln  /t:Build /p:Configuration=Debug
nuget pack ..\code\AlexPovar.ReSharperHelpers.nuspec -BasePath ..\code\AlexPovar.ReSharperHelpers\bin\Debug\net45 -Version %version%