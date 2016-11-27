using System.Reflection;
using System.Windows;
using JetBrains.DataFlow;
using JetBrains.IDE.RunConfig;
using JetBrains.ProjectModel;

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

      //Use reflection to set field to avoid reference to VS specific stuff.
      var projectVisibilityProp = commonEditor.GetType().GetProperty("ProjectRequiredVisibility", BindingFlags.Instance | BindingFlags.Public);
      if (projectVisibilityProp != null)
      {
        var visibility = projectVisibilityProp.GetValue(commonEditor) as Property<Visibility>;
        if (visibility != null)
        {
          visibility.Value = Visibility.Collapsed;
        }
      }

      return null;
    }
  }
}