using System;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;

namespace AlexPovar.ResharperTweaks.ContextActions.Pure
{
  [ContextAction(Group = "C#", Name = "[Tweaks] Set Pure attribute", Description = "Sets Pure annotation attribute.")]
  public class PureAttributeAction : PureAttributeActionBase
  {
    public PureAttributeAction([NotNull] ICSharpContextActionDataProvider provider) : base(provider)
    {
    }

    public override string Text => "Pure";

    protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
      var methodDeclaration = Provider.GetSelectedElement<IMethodDeclaration>();
      if (methodDeclaration == null) return null;

      AnnotationsUtil.CreateAndAddAnnotationAttribute(methodDeclaration, this.PureAttributeShortName);

      return null;
    }

    protected override bool ResolveIsAvailable(bool isAlreadyDeclared, IMethod method)
    {
      return !isAlreadyDeclared && IsRelevantForReturnTypeMethod(method);
    }

    private bool IsRelevantForReturnTypeMethod(IMethod method)
    {
      var retType = method.ReturnType;

      if (!retType.IsResolved) return false;

      if (retType.IsVoid()) return false;

      if (method.IsAsync && !retType.IsGenericTask()) return false;

      return true;
    }
  }
}