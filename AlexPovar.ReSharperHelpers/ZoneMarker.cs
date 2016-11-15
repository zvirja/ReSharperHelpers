using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Feature.Services;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.VsIntegration.Shell.Zones;

namespace AlexPovar.ReSharperHelpers
{
  [ZoneMarker]
  public class ZoneMarker : IRequire<ICodeEditingZone>, IRequire<ILanguageCSharpZone>, IRequire<DaemonZone>, IRequire<IVisualStudioZone>
  {
  }
}