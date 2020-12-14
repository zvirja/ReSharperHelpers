public class BuildVersionInfo
{
  public string AssemblyVersion;
  public string FileVersion;
  public string InfoVersion;
  public string NuGetVersion;

  public override string ToString() => $"Assembly: {AssemblyVersion}, Info: {InfoVersion}, File: {FileVersion} NuGet: {NuGetVersion}";
}