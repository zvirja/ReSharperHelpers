using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.Application.Settings.WellKnownRootKeys;
using JetBrains.ReSharper.Psi.EditorConfig;

namespace AlexPovar.ReSharperHelpers.Settings
{
  [SettingsKey(typeof(EnvironmentSettings), "Alex Povar ReSharper Helper settings")]
  [EditorConfigKey("resharperhelpers")]
  public class ReSharperHelperSettings
  {
    [CanBeNull, SettingsEntry("", "Tests project name")]
    [EditorConfigEntryAlias("tests_project_name", EditorConfigAliasType.Generalized)]
    public string TestsProjectName { get; set; }

    [CanBeNull, SettingsEntry("", "A sub-namespace to use within a test project as a root")]
    [EditorConfigEntryAlias("tests_project_sub_namespace", EditorConfigAliasType.Generalized)]
    public string TestsProjectSubNamespace { get; set; }

    [CanBeNull, SettingsEntry("Tests", "Test class name suffix")]
    [EditorConfigEntryAlias("new_test_class_name_suffix", EditorConfigAliasType.Generalized)]
    public string TestClassNameSuffix { get; set; }

    [CanBeNull, SettingsEntry("Test, Fixture", "Valid test class name suffixes")]
    [EditorConfigEntryAlias("existing_test_class_name_suffixes", EditorConfigAliasType.Generalized)]
    public string ValidTestClassNameSuffixes { get; set; }

    [NotNull]
    public static ReSharperHelperSettings GetSettings([NotNull] IContextBoundSettingsStore store)
    {
      return store.GetKey<ReSharperHelperSettings>(SettingsOptimization.OptimizeDefault);
    }
  }
}
