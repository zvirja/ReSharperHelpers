using JetBrains.Annotations;
using JetBrains.Application.Settings;

namespace AlexPovar.ReSharperHelpers.Settings
{
  [SettingsKey(typeof(EnvironmentSettings), "Alex Povar ReSharper Helper settings")]
  public class ReSharperHelperSettings
  {
    [CanBeNull, SettingsEntry("", "Tests project name")]
    public string TestsProjectName { get; set; }
  }
}