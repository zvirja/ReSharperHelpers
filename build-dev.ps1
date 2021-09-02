$devHostId = Get-Content -Path 'deploy.host' -ErrorAction Ignore

./build.ps1 --no-logo --target CompleteBuild --build-config Debug --dev-host-id $devHostId --build-version dev --skip Test
