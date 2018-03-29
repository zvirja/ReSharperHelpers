using System.Collections.Generic;
using AlexPovar.ReSharperHelpers.Highlighting;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.Components;
using JetBrains.Application.Environment;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.ProjectModel.Settings.Cache;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TestFramework.Components.Daemon;

namespace AlexPovar.ReSharperHelpers.Tests.Highlighting
{
  [ShellComponent]
  public class FixForSuppressingHighlightingSettingsManager : SuppressingHighlightingSettingsManager, IHideImplementation<TestHighlightingSettingsManagerImpl>
  {
    public FixForSuppressingHighlightingSettingsManager(
      [NotNull] Lifetime lifetime,
      [NotNull] ShellPartCatalogSet partsCatalogueSet,
      [NotNull] ILanguages allLanguages,
      [NotNull] ISettingsStore settingsStore,
      [NotNull] IEnumerable<ICustomConfigurableSeverityItemProvider> customConfigurableSeverityItemProviders,
      [NotNull] IEnumerable<ILiveCustomConfigurableSeverityItemProvider> liveCustomConfigurableSeverityItemProviders,
      [NotNull] SettingsCacheManager cacheManger)
      : base(lifetime, partsCatalogueSet, allLanguages, settingsStore, customConfigurableSeverityItemProviders, liveCustomConfigurableSeverityItemProviders, cacheManger)
    {
    }
  }
}