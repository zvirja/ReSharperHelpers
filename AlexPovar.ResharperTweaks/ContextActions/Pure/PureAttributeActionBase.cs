using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.Util;

namespace AlexPovar.ResharperTweaks.ContextActions.Pure
{
  public abstract class PureAttributeActionBase : ContextActionBase
  {
    [NotNull] protected readonly ICSharpContextActionDataProvider Provider;

    protected PureAttributeActionBase([NotNull] ICSharpContextActionDataProvider provider)
    {
      if (provider == null) throw new ArgumentNullException(nameof(provider));

      Provider = provider;
      PureAttributeShortName = CodeAnnotationsCache.PureAttributeShortName;
    }

    [NotNull]
    protected string PureAttributeShortName { get; }

    public override IEnumerable<IntentionAction> CreateBulbItems() => new[] {this.ToAnnotateAction()};

    public override bool IsAvailable(IUserDataHolder cache)
    {
      var methodName = Provider.GetSelectedElement<ICSharpIdentifier>();
      var methodDeclaration = methodName?.Parent as IMethodDeclaration;

      var declaredMethod = methodDeclaration?.DeclaredElement;
      if (declaredMethod == null) return false;

      //Is null if Annotations are not installed
      var pureAttributeType = methodDeclaration.GetPsiServices().GetCodeAnnotationsCache().GetAttributeTypeForElement(methodDeclaration, PureAttributeShortName);

      if (pureAttributeType == null) return false;

      var isAlreadyDeclared = declaredMethod.HasAttributeInstance(pureAttributeType.GetClrName(), false);

      return ResolveIsAvailable(isAlreadyDeclared, declaredMethod);
    }

    protected abstract bool ResolveIsAvailable(bool isAlreadyDeclared, IMethod method);
  }
}