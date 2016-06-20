using System;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.TextControl;
using JetBrains.Util;

namespace AlexPovar.ResharperTweaks.ContextActions.AutoFixture
{
  public abstract class AutoFixtureAttributeAction : TweaksContextActionBase
  {
    protected AutoFixtureAttributeAction(ICSharpContextActionDataProvider provider, string attributeTypeName)
    {
      this.Provider = provider;
      this.AttributeType = new ClrTypeName(attributeTypeName);

      this.IsEnabled = this.IsAutoFixtureXunitReferenced(provider.PsiModule);
    }

    private ICSharpContextActionDataProvider Provider { get; }

    private IClrTypeName AttributeType { get; }

    private bool IsEnabled { get; }

    public override bool IsAvailable(IUserDataHolder cache)
    {
      if (!this.IsEnabled) return false;

      var parameterDeclaration = this.Provider.GetSelectedElement<ICSharpParameterDeclaration>() as IRegularParameterDeclaration;

      if (parameterDeclaration == null || !parameterDeclaration.IsValid()) return false;

      var parameter = parameterDeclaration.DeclaredElement;
      if (parameter == null) return false;

      var alreadyPresent = parameter.GetAttributeInstances(this.AttributeType, false).Any();

      return !alreadyPresent;
    }

    protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
      var parameterDeclaration = this.Provider.GetSelectedElement<ICSharpParameterDeclaration>() as IRegularParameterDeclaration;
      if (parameterDeclaration == null || !parameterDeclaration.IsValid()) return null;

      var psiModule = this.Provider.PsiModule;

      var symbolScope = parameterDeclaration.GetPsiServices().Symbols.GetSymbolScope(psiModule, true, true);
      var frozenAttributeType = symbolScope.GetTypeElementByCLRName(this.AttributeType);

      if (frozenAttributeType == null) return null;

      var attribute = CSharpElementFactory.GetInstance(psiModule).CreateAttribute(frozenAttributeType);
      parameterDeclaration.AddAttributeBefore(attribute, null);

      return null;
    }

    private bool IsAutoFixtureXunitReferenced(IPsiModule module)
    {
      return module.GetPsiServices().Modules.GetModuleReferences(module).Any(refModule => refModule.Module.Name.Equals("Ploeh.AutoFixture.Xunit2", StringComparison.Ordinal));
    }
  }
}