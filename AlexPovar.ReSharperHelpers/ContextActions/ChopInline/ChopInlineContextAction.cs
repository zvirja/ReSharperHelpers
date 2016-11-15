using System.Collections.Generic;
using AlexPovar.ReSharperHelpers.Helpers;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.UI.BulbMenu;
using JetBrains.Util;

namespace AlexPovar.ReSharperHelpers.ContextActions.ChopInline
{
  [ContextAction(Group = "C#", Name = "[AlexHelpers] Chop method arguments", Description = "Chops method arguments", Priority = short.MinValue)]
  public class ChopInlineContextAction : IContextAction
  {
    [NotNull] private readonly ICSharpContextActionDataProvider _myProvider;

    public ChopInlineContextAction([NotNull] ICSharpContextActionDataProvider provider)
    {
      this._myProvider = provider;
    }

    [CanBeNull]
    private IMethodDeclaration ContextMethodDeclaration { get; set; }

    public IEnumerable<IntentionAction> CreateBulbItems()
    {
      if (this.ContextMethodDeclaration == null) yield break;

      var anchor = new SubmenuAnchor(HelperActionsConstants.ContextActionsAnchor, SubmenuBehavior.Executable);

      var chopAction = new ChopMethodArgumentsAction(this.ContextMethodDeclaration).ToContextActionIntention(anchor, MyIcons.ContextActionIcon);
      var oneLineAction = new OnelineMethodArgumentsAction(this.ContextMethodDeclaration).ToContextActionIntention(anchor, MyIcons.ContextActionIcon);

      yield return chopAction;
      yield return oneLineAction;
    }

    public bool IsAvailable(IUserDataHolder cache)
    {
      var methodName = this._myProvider.GetSelectedElement<ICSharpIdentifier>();
      var methodDeclaration = MethodDeclarationNavigator.GetByNameIdentifier(methodName);

      if (methodDeclaration == null) return false;
      if (methodDeclaration.ParameterDeclarations.IsEmpty) return false;

      this.ContextMethodDeclaration = methodDeclaration;
      return true;
    }
  }
}