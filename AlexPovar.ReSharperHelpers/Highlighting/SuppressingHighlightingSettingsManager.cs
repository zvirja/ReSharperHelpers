using System.Collections.Generic;
using System.Linq;
using AlexPovar.ReSharperHelpers.Helpers;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.Environment;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.ProjectModel.Settings.Cache;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.Util;

namespace AlexPovar.ReSharperHelpers.Highlighting
{
  [ShellComponent]
  public class SuppressingHighlightingSettingsManager : HighlightingSettingsManagerImpl, IHighlightingSettingsManager
  {
    private const string AttributeShortNameFull = "SuppressHighlightingAttribute";
    private const string AttributeShortNameShort = "SuppressHighlighting";

    [NotNull] private static readonly Key<ISet<string>> SuppressedHighlightingKey = new Key<ISet<string>>("ReSharperHelpers.SuppressedHighlightings");

    [NotNull] private static readonly ConfigurableSeverityItem DisabledItem = new ConfigurableSeverityItem(null, null, null, null, null, Severity.DO_NOT_SHOW, false, false, null);

    public SuppressingHighlightingSettingsManager(
      [NotNull] Lifetime lifetime,
      [NotNull] ShellPartCatalogSet partsCatalogSet,
      [NotNull] ILanguages allLanguages,
      [NotNull] ISettingsStore settingsStore,
      [NotNull] IEnumerable<ICustomConfigurableSeverityItemProvider> customConfigurableSeverityItemProviders,
      [NotNull] SettingsCacheManager cacheManger)
      : base(lifetime, partsCatalogSet, allLanguages, settingsStore, customConfigurableSeverityItemProviders, cacheManger)
    {
    }

    public new Severity GetSeverity(IHighlighting highlighting, IPsiSourceFile sourceFile)
    {
      using (ThreadStack<IPsiModule>.EnterScope(sourceFile?.PsiModule))
      {
        return base.GetSeverity(highlighting, sourceFile);
      }
    }

    public override ConfigurableSeverityItem GetSeverityItem(string id)
    {
      var file = ThreadStack<IPsiModule>.Current;
      var disabledHighlightingsForProject = GetSuppressedHighlightingSet(file);

      if (disabledHighlightingsForProject?.Contains(id) == true)
      {
        return DisabledItem;
      }

      return base.GetSeverityItem(id);
    }


    [CanBeNull]
    private static ISet<string> GetSuppressedHighlightingSet([CanBeNull] IPsiModule module)
    {
      return module?.GetOrCreateData(SuppressedHighlightingKey, module, ResolveSuppressedHighlightingsByAssemblyAttributes);
    }

    private static bool IsValidAttributeShortName([NotNull] string shortAttributeName)
    {
      return shortAttributeName == AttributeShortNameShort || shortAttributeName == AttributeShortNameFull;
    }

    [NotNull]
    private static ISet<string> ResolveSuppressedHighlightingsByAssemblyAttributes([NotNull] IPsiModule module)
    {
      return module.GetPsiServices().Symbols.GetModuleAttributes(module).GetAttributeInstances(false)
        .Where(inst => IsValidAttributeShortName(inst.GetClrName().ShortName) && inst.PositionParameterCount == 1)
        .Select(inst => inst.PositionParameter(0))
        .Where(p => p.IsConstant && p.ConstantValue.IsString())
        .Select(p => (string)p.ConstantValue.Value)
        .ToSet();
    }
  }
}