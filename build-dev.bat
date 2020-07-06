set /p DevHostId=<deploy.host
call build.cmd --target CompleteBuild --build-config Debug --skip-tests --dev-host-id %DevHostId% --build-version dev
pause
