using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;

namespace AlexPovar.ReSharperHelpers.ContextActions.ChopInline
{
  public abstract class ChopInlineMethodActionBase : BulbActionBase
  {
    [NotNull] private readonly IMethodDeclaration _methodDeclaration;

    protected ChopInlineMethodActionBase([NotNull] IMethodDeclaration methodDeclaration)
    {
      this._methodDeclaration = methodDeclaration;
    }

    private static bool IsLineBreak([NotNull] ITreeNode node)
    {
      return node.NodeType == CSharpTokenType.NEW_LINE;
    }

    private void DoCleanupLineBreaks([NotNull] ICSharpParametersOwnerDeclaration methodDeclaration, [NotNull] IFormalParameterList parameters)
    {
      var nodesToRemove = new List<ITokenNode>();

      //Remove line breaks between parenthesis and arguments.
      for (var node = methodDeclaration.LPar; node != null && node != methodDeclaration.RPar; node = node.GetNextToken())
      {
        if (IsLineBreak(node))
        {
          nodesToRemove.Add(node);
        }
      }

      nodesToRemove.ForEach(LowLevelModificationUtil.DeleteChild);

      //Remove all line breaks between arguments.
      nodesToRemove.Clear();
      nodesToRemove.AddRange(parameters.Children().OfType<ITokenNode>().Where(IsLineBreak));

      nodesToRemove.ForEach(LowLevelModificationUtil.DeleteChild);
    }

    protected virtual void DoPutNewIndents([NotNull] IFormalParameterList parameters)
    {
      LowLevelModificationUtil.AddChildBefore(parameters, CreateLineBreakToken());

      foreach (var paramDecl in parameters.ParameterDeclarationsEnumerable.Skip(1))
      {
        LowLevelModificationUtil.AddChildBefore(paramDecl, CreateLineBreakToken());
      }
    }

    [NotNull]
    private static LeafElementBase CreateLineBreakToken()
    {
      return CSharpTokenType.NEW_LINE.CreateLeafElement();
    }

    protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
      var methodDeclaration = this._methodDeclaration;

      var parameters = methodDeclaration.Params;
      if (parameters == null) return null;

      using (WriteLockCookie.Create())
      {
        this.DoCleanupLineBreaks(methodDeclaration, parameters);
        this.DoPutNewIndents(parameters);
      }

      parameters.Language.LanguageService()?.CodeFormatter?.Format(parameters, CodeFormatProfile.DEFAULT, progress);
      return null;
    }
  }
}