using System;
using System.Linq.Expressions;
using JetBrains.Application.Settings;

namespace AlexPovar.ReSharperHelpers.Settings
{
  [SettingsKey(typeof(EnvironmentSettings), "Alex Povar ReSharper Helper setting")]
  public class ReSharperHelperSettings
  {
    [SettingsEntry("", "Tests project name")]
    public string TestsProjectName { get; set; }
  }

  public static class ReSharperHelperSettingsAccessor
  {
    public static readonly Expression<Func<ReSharperHelperSettings, string>>
      TestsProjectName = settings => settings.TestsProjectName;
  }
}