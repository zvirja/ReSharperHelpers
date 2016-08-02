using System;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;

namespace AlexPovar.ReSharperHelpers.ContextActions.Pure
{
  [ContextAction(Group = "C#", Name = "[AlexHelpers] Remove Pure attribute", Description = "Removes Pure annotation attribute.")]
  public class NotPureAttributeAction : PureAttributeActionBase
  {
    public NotPureAttributeAction([NotNull] ICSharpContextActionDataProvider provider) : base(provider)
    {
    }


    public override string Text => "Not pure";

    protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
      var methodDeclaration = this.Provider.GetSelectedElement<IMethodDeclaration>();
      if (methodDeclaration == null) return null;

      var codeAnnotationCache = methodDeclaration.GetPsiServices().GetCodeAnnotationsCache();

      var pureAttribute = methodDeclaration.AttributesEnumerable.FirstOrDefault(attr => codeAnnotationCache.IsAnnotationAttribute(attr.GetAttributeInstance(), this.PureAttributeShortName));

      if (pureAttribute == null) return null;

      methodDeclaration.RemoveAttribute(pureAttribute);

      return null;
    }

    protected override bool ResolveIsAvailable(bool isAlreadyDeclared, IMethod method) => isAlreadyDeclared;
  }
}