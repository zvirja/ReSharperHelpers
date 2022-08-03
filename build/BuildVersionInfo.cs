#nullable enable

public record BuildVersionInfo(string AssemblyVersion, string FileVersion, string InfoVersion, string NuGetVersion)
{
   public static BuildVersionInfo Create(
      string baseVersion,
      string? assemblyVersion = null,
      string? fileVersion = null,
      string? infoVersion = null,
      string? nuGetVersion = null)
   {
      return new BuildVersionInfo(
         assemblyVersion ?? baseVersion,
         fileVersion ?? baseVersion,
         infoVersion ?? baseVersion,
         nuGetVersion ?? baseVersion);
   }

   public override string ToString() => $"Assembly: {AssemblyVersion}, Info: {InfoVersion}, File: {FileVersion} NuGet: {NuGetVersion}";
}
