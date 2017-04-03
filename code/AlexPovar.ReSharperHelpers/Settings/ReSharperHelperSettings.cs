using JetBrains.Annotations;
using JetBrains.Application.Settings;

namespace AlexPovar.ReSharperHelpers.Settings
{
  [SettingsKey(typeof(EnvironmentSettings), "Alex Povar ReSharper Helper settings")]
  public class ReSharperHelperSettings
  {
    [CanBeNull, SettingsEntry("", "Tests project name")]
    public string TestsProjectName { get; set; }

    [CanBeNull, SettingsEntry("Tests", "Test class name suffix")]
    public string TestClassNameSuffix { get; set; }

    [CanBeNull, SettingsEntry("Test, Fixture", "Valid test class name suffixes")]
    public string ValidTestClassNameSuffixes { get; set; }

    [NotNull]
    public static ReSharperHelperSettings GetSettings([NotNull] IContextBoundSettingsStore store)
    {
      return store.GetKey<ReSharperHelperSettings>(SettingsOptimization.OptimizeDefault);
    }
  }
}