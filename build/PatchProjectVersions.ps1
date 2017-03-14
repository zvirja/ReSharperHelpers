param (
    [String[]] $projectFiles,
    [Parameter(Mandatory=$false)] $buildVersion = $env:BUILD_VERSION,
    [Parameter(Mandatory=$false)] $rootDir = $env:APPVEYOR_BUILD_FOLDER
)

foreach($proj in $projectFiles) {
    $fullProjPath = Join-Path $rootDir $proj

    # Read .csproj file as xml
    $xml = [xml](Get-Content $fullProjPath)
    $propertyGroup = $xml.Project.PropertyGroup

    # Patch versions
    $propertyGroup.Version = $buildVersion
    $propertyGroup.AssemblyVersion = $buildVersion
    $propertyGroup.FileVersion = $buildVersion

    $xml.Save($fullProjPath)

    Write-Host "Patched version for project. Path: $fullProjPath, Version: $buildVersion"
}