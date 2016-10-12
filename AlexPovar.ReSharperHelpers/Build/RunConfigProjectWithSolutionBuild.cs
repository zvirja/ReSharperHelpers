using JetBrains.IDE.RunConfig;

namespace AlexPovar.ReSharperHelpers.Build
{
  public class RunConfigProjectWithSolutionBuild : RunConfigBase
  {
    public override void Execute(RunConfigContext context)
    {
      context.ExecutionProvider.Execute(null);
    }
  }
}