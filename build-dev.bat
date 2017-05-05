set /p DevHostId=<deploy.host
call build.cmd CompleteBuild BuildConfig="Debug" RunTests="false" DevHostId=%DevHostId%
pause