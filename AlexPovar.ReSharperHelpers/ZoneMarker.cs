using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Feature.Services;
using JetBrains.ReSharper.Psi.CSharp;

namespace AlexPovar.ReSharperHelpers
{
  [ZoneMarker]
  public class ZoneMarker : IRequire<ICodeEditingZone>, IRequire<ILanguageCSharpZone>
  {
  }
}