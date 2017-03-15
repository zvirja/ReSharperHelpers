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
      this.TestClassNameSuffix = new Property<string>(lifetime, nameof(ReSharperHelperSettings.TestClassNameSuffix));
      this.ValidTestClassNameSuffixes = new Property<string>(lifetime, nameof(ReSharperHelperSettings.ValidTestClassNameSuffixes));

      settingsStore.SetBinding(lifetime, (ReSharperHelperSettings settings) => settings.TestsProjectName, this.TestProjectName);
      settingsStore.SetBinding(lifetime, (ReSharperHelperSettings settings) => settings.TestClassNameSuffix, this.TestClassNameSuffix);
      settingsStore.SetBinding(lifetime, (ReSharperHelperSettings settings) => settings.ValidTestClassNameSuffixes, this.ValidTestClassNameSuffixes);
    }

    [NotNull]
    public IProperty<string> TestClassNameSuffix { get; }

    [NotNull]
    public IProperty<string> ValidTestClassNameSuffixes { get; }

    [NotNull]
    public IProperty<string> TestProjectName { get; }
  }
}