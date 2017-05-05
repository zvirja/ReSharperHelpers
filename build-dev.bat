set /p DevHostId=<deploy.host
call build.cmd CompleteBuild BuildConfig="Debug" RunTests="false" DevHostId=%DevHostId% BuildVersion="1.0.0.0"
pause