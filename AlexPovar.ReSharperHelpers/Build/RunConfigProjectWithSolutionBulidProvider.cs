using JetBrains.Application;
using JetBrains.IDE.RunConfig;
using JetBrains.UI.Icons;
using JetBrains.VsIntegration.Resources;

namespace AlexPovar.ReSharperHelpers.Build
{
  [ShellComponent]
  public class RunConfigProjectWithSolutionBulidProvider : RunConfigProviderBase
  {
    public override string Name => "Launch solution";

    public override string Type => "launch solution";

    public override IconId IconId => RunConfigThemedIcons.RunConfigProject.Id;

    public override IRunConfig CreateNew()
    {
      return new RunConfigProjectWithSolutionBuild
      {
        Type = this.Type
      };
    }
  }
}