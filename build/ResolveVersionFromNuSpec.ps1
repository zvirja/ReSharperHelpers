$nugetNs = @{n = 'http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd'}
$node = Select-Xml -XPath "/n:package/n:metadata/n:version" .\AlexPovar.ReSharperHelpers.nuspec -Namespace $nugetNs

$currentVersion = $node.Node.InnerText;
$env:BUILD_VERSION = [regex]::match($currentVersion, '(\d.){1,2}\d').Value;

if(-NOT $env:BUILD_VERSION)
{
  throw "Unable to resolve version."
}

$env:BUILD_VERSION = "$env:BUILD_VERSION.$env:APPVEYOR_BUILD_NUMBER";