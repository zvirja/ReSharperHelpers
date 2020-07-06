set /p DevHostId=<deploy.host
call build.cmd --target CompleteBuild --build-config Debug --dev-host-id %DevHostId% --build-version dev --skip Test
pause
