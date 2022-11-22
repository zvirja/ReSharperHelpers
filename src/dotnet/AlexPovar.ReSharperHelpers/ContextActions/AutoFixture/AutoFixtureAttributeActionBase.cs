using System;
using System.Collections.Generic;
using System.Linq;
using AlexPovar.ReSharperHelpers.Helpers;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;
using JetBrains.Util;

namespace AlexPovar.ReSharperHelpers.ContextActions.AutoFixture
{
  public abstract class AutoFixtureAttributeActionBase : ContextActionBase
  {
    protected AutoFixtureAttributeActionBase([NotNull] ICSharpContextActionDataProvider provider, [NotNull] IClrTypeName attributeType)
    {
      this.Provider = provider;
      this.AttributeType = attributeType;
    }

    [NotNull]
    protected ICSharpContextActionDataProvider Provider { get; }

    [NotNull]
    private IClrTypeName AttributeType { get; }

    [CanBeNull]
    private ITypeElement ResolvedAttributeType { get; set; }

    public override bool IsAvailable(IUserDataHolder cache)
    {
      var parameter = this.Provider.GetSelectedElement<IRegularParameterDeclaration>()?.DeclaredElement;
      if (parameter == null || !parameter.IsValid()) return false;

      // Check whether AF is present.
      this.ResolvedAttributeType = TypeElementUtil.GetTypeElementByClrName(this.AttributeType, this.Provider.PsiModule);
      if (this.ResolvedAttributeType == null) return false;

      return this.IsAvailableWithAttributeInstances(parameter.GetAttributeInstances(this.AttributeType, false));
    }

    public override IEnumerable<IntentionAction> CreateBulbItems()
    {
      return this.ToHelpersContextActionIntentions();
    }

    protected virtual bool IsAvailableWithAttributeInstances([NotNull] IList<IAttributeInstance> existingAttributes)
    {
      return existingAttributes.IsEmpty();
    }

    protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
      var parameterDeclaration = this.Provider.GetSelectedElement<IRegularParameterDeclaration>();
      if (parameterDeclaration == null) return null;

      var existingAttributes = parameterDeclaration.AttributesEnumerable
        .Where(attr => attr.GetAttributeInstance().GetClrName().Equals(this.AttributeType))
        .ToArray();

      foreach (var existingAttribute in existingAttributes)
      {
        parameterDeclaration.RemoveAttribute(existingAttribute);
      }


      var psiModule = this.Provider.PsiModule;

      // Try resolve again. It could happen that IsAvailable method was not invoked (e.g. for Frozen(Matching...)).
      this.ResolvedAttributeType = this.ResolvedAttributeType ?? TypeElementUtil.GetTypeElementByClrName(this.AttributeType, this.Provider.PsiModule);
      if (this.ResolvedAttributeType == null) return null;

      var attribute = this.CreateAttribute(this.ResolvedAttributeType, CSharpElementFactory.GetInstance(parameterDeclaration), psiModule);
      if (attribute != null)
      {
        parameterDeclaration.AddAttributeBefore(attribute, null);
      }

      return null;
    }

    [CanBeNull]
    protected virtual IAttribute CreateAttribute([NotNull] ITypeElement resolvedAttributeType, [NotNull] CSharpElementFactory factory, [NotNull] IPsiModule psiModule)
    {
      return factory.CreateAttribute(resolvedAttributeType);
    }
  }
}
