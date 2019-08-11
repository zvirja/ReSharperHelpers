@echo off
cls

IF NOT EXIST "build\tools\FAKE.Core\tools\Fake.exe" (
  "tools\nuget\nuget.exe" install "NUnit.Runners" -OutputDirectory "build\tools" -ExcludeVersion -Version 3.10.0
  "tools\nuget\nuget.exe" install "FAKE.Core" -OutputDirectory "build\tools" -ExcludeVersion -Version 4.64.17
)

"build\tools\FAKE.Core\tools\Fake.exe" build.fsx %*