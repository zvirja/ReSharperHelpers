using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Feature.Services;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework.Application.Zones;

namespace AlexPovar.ReSharperHelpers.Tests
{
  [ZoneDefinition]
  public interface IReSharperHelpersTestEnvironmentZone : ITestsEnvZone, IRequire<PsiFeatureTestZone>, IRequire<ICodeEditingZone>, IRequire<ILanguageCSharpZone>
  {
  }

  [ZoneMarker]
  public class ZoneMarker : IRequire<IReSharperHelpersTestEnvironmentZone>
  {
  }
}