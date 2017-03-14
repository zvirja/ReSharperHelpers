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
    if($propertyGroup.Version) { $propertyGroup.Version = $buildVersion }
    if($propertyGroup.AssemblyVersion) { $propertyGroup.AssemblyVersion = $buildVersion }
    if($propertyGroup.FileVersion) { $propertyGroup.FileVersion = $buildVersion }

    $xml.Save($fullProjPath)

    Write-Host "Patched version for core project:`n    Path: $fullProjPath`n    Version: $buildVersion`n" -ForegroundColor "Green"
}