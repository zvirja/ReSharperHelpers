using System;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

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

      var codeAnnotationCache = methodDeclaration.GetPsiServices().GetCodeAnnotationsCache();

      var pureAttributeType = codeAnnotationCache.GetAttributeTypeForElement(methodDeclaration, PureAttributeShortName);
      if (pureAttributeType == null) return null;

      var pureAttribute = Provider.ElementFactory.CreateAttribute(pureAttributeType);

      var lastAnnotationAttribute = methodDeclaration.AttributesEnumerable.LastOrDefault(attr =>
      {
        var attrInstanceType = attr.GetAttributeInstance().GetClrName();
        return codeAnnotationCache.IsAnnotationType(attrInstanceType, attrInstanceType.ShortName);
      });

      if (lastAnnotationAttribute == null)
      {
        methodDeclaration.AddAttributeAfter(pureAttribute, null);
      }
      else
      {
        var attrList = (IAttributeList) lastAnnotationAttribute.Parent.NotNull("Attribute parent cannot be null.");
        attrList.AddAttributeAfter(pureAttribute, lastAnnotationAttribute);
      }

      return null;
    }

    protected override bool ResolveIsAvailable(bool isAlreadyDeclared) => !isAlreadyDeclared;
  }
}