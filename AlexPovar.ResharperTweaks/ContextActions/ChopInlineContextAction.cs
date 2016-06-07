using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.UI.BulbMenu;
using JetBrains.Util;

namespace AlexPovar.ResharperTweaks.ContextActions
{
  [ContextAction(Group = "C#", Name = "[Tweaks] Chop method arguments", Description = "Chops method arguments", Priority = short.MinValue)]
  public class ChopInlineContextAction : IContextAction
  {
    [NotNull] private readonly ICSharpContextActionDataProvider _myProvider;

    public ChopInlineContextAction(ICSharpContextActionDataProvider provider)
    {
      _myProvider = provider;
    }

    [CanBeNull]
    private IMethodDeclaration ContextMethodDeclaration { get; set; }

    public IEnumerable<IntentionAction> CreateBulbItems()
    {
      var anchor = new ExecutableGroupAnchor(IntentionsAnchors.ContextActionsAnchor, null, false);

      if (ContextMethodDeclaration == null) yield break;

      var actions = new ChopMethodArgumentsAction(ContextMethodDeclaration).ToContextAction(anchor);
      actions = actions.Concat(new OnelineMethodArgumentsAction(ContextMethodDeclaration).ToContextAction(anchor));

      foreach (var action in actions) yield return action;
    }

    public bool IsAvailable(IUserDataHolder cache)
    {
      var methodName = _myProvider.GetSelectedElement<ICSharpIdentifier>();

      var methodDeclaration = methodName?.Parent as IMethodDeclaration;

      if (methodDeclaration == null) return false;

      ContextMethodDeclaration = methodDeclaration;
      return true;
    }
  }
}