using System.Windows;
using JetBrains.DataFlow;
using JetBrains.IDE.RunConfig;
using JetBrains.ProjectModel;
using JetBrains.VsIntegration.IDE.RunConfig;

namespace AlexPovar.ReSharperHelpers.Build
{
  public class RunConfigProjectWithSolutionBuild : RunConfigBase
  {
    public override void Execute(RunConfigContext context)
    {
      context.ExecutionProvider.Execute(null);
    }

    public override IRunConfigEditorAutomation CreateEditor(Lifetime lifetime, IRunConfigCommonAutomation commonEditor, ISolution solution)
    {
      //Configure commot editor to hide build options. We support solution build only.
      commonEditor.IsWholeSolutionChecked.Value = true;
      commonEditor.WholeSolutionVisibility.Value = Visibility.Collapsed;
      commonEditor.IsSpecificProjectChecked.Value = false;

      var casted = commonEditor as RunConfigCommonAutomation;
      if (casted != null)
      {
        casted.ProjectRequiredVisibility.Value = Visibility.Collapsed;
      }

      return null;
    }
  }
}