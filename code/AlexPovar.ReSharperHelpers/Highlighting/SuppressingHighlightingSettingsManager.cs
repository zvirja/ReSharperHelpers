using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.Environment;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
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
    [NotNull] private static readonly IClrTypeName SuppressAttributeName = new ClrTypeName("System.Diagnostics.CodeAnalysis.SuppressMessageAttribute");

    [NotNull] private static readonly Key<ISet<string>> SuppressedHighlightingKey = new Key<ISet<string>>("ReSharperHelpers.SuppressedHighlightings");

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
      var module = sourceFile?.PsiModule;
      return this.TryGetHighlightingSeverity(highlighting, module) ?? base.GetSeverity(highlighting, sourceFile);
    }

    public new Severity GetSeverity(IHighlighting highlighting, ISolution solution)
    {
      var module = solution?.GetComponent<DocumentManager>().TryGetProjectFile(highlighting.CalculateRange().Document)?.GetPsiModule();
      return this.TryGetHighlightingSeverity(highlighting, module) ?? base.GetSeverity(highlighting, solution);
    }

    private Severity? TryGetHighlightingSeverity([NotNull] IHighlighting highlighting, [CanBeNull] IPsiModule module)
    {
      if (module == null) return null;

      var configurableId = highlighting.GetConfigurableSeverityId(this.GetHighlightingAttribute(highlighting));

      if (GetSuppressedHighlightingsForModule(module).Contains(configurableId))
      {
        return Severity.DO_NOT_SHOW;
      }

      return null;
    }

    [NotNull]
    private static ISet<string> GetSuppressedHighlightingsForModule([NotNull] IPsiModule module)
    {
      return module.GetOrCreateData(SuppressedHighlightingKey, module, ResolveSuppressedHighlightingsByAssemblyAttributes);
    }

    private static bool IsValidAttributeInstance([NotNull] IAttributeInstance instance)
    {
      if (instance.PositionParameterCount != 2) return false;

      var categoryParam = instance.PositionParameter(0);
      if (!categoryParam.IsConstant || !categoryParam.ConstantValue.IsString() || (string)categoryParam.ConstantValue.Value != "ReSharper") return false;

      var idParam = instance.PositionParameter(1);
      return idParam.IsConstant && idParam.ConstantValue.IsString();
    }

    [NotNull]
    private static ISet<string> ResolveSuppressedHighlightingsByAssemblyAttributes([NotNull] IPsiModule module)
    {
      return module.GetPsiServices()
        .Symbols.GetModuleAttributes(module)
        .GetAttributeInstances(SuppressAttributeName, false)
        .Where(IsValidAttributeInstance)
        .Select(inst => (string)inst.PositionParameter(1).ConstantValue.Value)
        .ToSet();
    }
  }
}