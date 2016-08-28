using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.UI.Options;

namespace AlexPovar.ReSharperHelpers.Settings
{
  public class ReSharperHelpersOptionsPageViewModel
  {
    public ReSharperHelpersOptionsPageViewModel([NotNull] Lifetime lifetime, [NotNull] OptionsSettingsSmartContext settingsStore)
    {
      this.TestProjectName = new Property<string>(lifetime, nameof(ReSharperHelperSettings.TestsProjectName));

      settingsStore.SetBinding(lifetime, (ReSharperHelperSettings settings) => settings.TestsProjectName, this.TestProjectName);
    }

    [NotNull]
    public IProperty<string> TestProjectName { get; }
  }
}