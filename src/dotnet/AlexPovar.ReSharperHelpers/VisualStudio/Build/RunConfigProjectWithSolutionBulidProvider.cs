using JetBrains.Application;
using JetBrains.Application.Parts;
using JetBrains.ProjectModel.Features.RunConfig;
using JetBrains.UI.Icons;

namespace AlexPovar.ReSharperHelpers.VisualStudio.Build
{
  [ShellComponent(Instantiation.DemandAnyThreadSafe)]
  public class RunConfigProjectWithSolutionBuildProvider : RunConfigProviderBase
  {
    public override string Name => "Launch solution";

    public override string Type => "launch solution";

    public override IconId IconId => MainThemedIcons.HelpersContextAction.Id;

    public override IRunConfig CreateNew()
    {
      return new RunConfigProjectWithSolutionBuild
      {
        Type = Type
      };
    }
  }
}
