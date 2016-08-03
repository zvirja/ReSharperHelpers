using System.Collections.Generic;
using System.Linq;
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

    public ChopInlineContextAction(ICSharpContextActionDataProvider provider)
    {
      this._myProvider = provider;
    }

    [CanBeNull]
    private ICSharpParametersOwnerDeclaration ContextMethodDeclaration { get; set; }

    public IEnumerable<IntentionAction> CreateBulbItems()
    {
      if (this.ContextMethodDeclaration == null) yield break;

      var anchor = new ExecutableGroupAnchor(HelperActionsConstants.ContextActionsAnchor, null, false);

      var actions = new ChopMethodArgumentsAction(this.ContextMethodDeclaration).ToContextAction(anchor, MyIcons.ContextActionIcon);
      actions = actions.Concat(new OnelineMethodArgumentsAction(this.ContextMethodDeclaration).ToContextAction(anchor, MyIcons.ContextActionIcon));

      foreach (var action in actions) yield return action;
    }

    public bool IsAvailable(IUserDataHolder cache)
    {
      var methodName = this._myProvider.GetSelectedElement<ICSharpIdentifier>();

      var methodDeclaration = methodName?.Parent as ICSharpParametersOwnerDeclaration;

      if (methodDeclaration == null) return false;
      if (methodDeclaration.ParameterDeclarations.IsEmpty) return false;

      this.ContextMethodDeclaration = methodDeclaration;
      return true;
    }
  }
}