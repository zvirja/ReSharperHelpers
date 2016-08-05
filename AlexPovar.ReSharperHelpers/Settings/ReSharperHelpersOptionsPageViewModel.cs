using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.UI.Options;

namespace AlexPovar.ReSharperHelpers.Settings
{
  public class ReSharperHelpersOptionsPageViewModel
  {
    public ReSharperHelpersOptionsPageViewModel(
      [NotNull] Lifetime lifetime, [NotNull] OptionsSettingsSmartContext settingsStore)
    {
      this.TestProjectName = new Property<string>(lifetime, nameof(ReSharperHelperSettings.TestsProjectName));

      settingsStore.SetBinding(lifetime, ReSharperHelperSettingsAccessor.TestsProjectName, this.TestProjectName);
    }

    public IProperty<string> TestProjectName { get; }
  }
}