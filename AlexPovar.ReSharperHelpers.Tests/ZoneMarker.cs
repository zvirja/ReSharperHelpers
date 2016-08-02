using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Feature.Services;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework.Application.Zones;

namespace AlexPovar.ReSharperHelpers.Tests
{
  [ZoneDefinition]
  public class ResharperHelpersTestEnvironmentZone : ITestsZone, IRequire<PsiFeatureTestZone>
  {
  }

  [ZoneMarker]
  public class ZoneMarker : IRequire<ICodeEditingZone>, IRequire<ILanguageCSharpZone>, IRequire<ResharperHelpersTestEnvironmentZone>
  {
  }
}