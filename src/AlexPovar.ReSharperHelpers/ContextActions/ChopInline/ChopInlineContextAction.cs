using System.Collections.Generic;
using AlexPovar.ReSharperHelpers.Helpers;
using JetBrains.Annotations;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.Util;

namespace AlexPovar.ReSharperHelpers.ContextActions.ChopInline
{
  [ContextAction(Group = "C#", Name = "[ReSharperHelpers] Chop method arguments", Description = "Chops method arguments", Priority = short.MinValue)]
  public class ChopInlineContextAction : IContextAction
  {
    [NotNull] private readonly ICSharpContextActionDataProvider _myProvider;

    public ChopInlineContextAction([NotNull] ICSharpContextActionDataProvider provider)
    {
      this._myProvider = provider;
    }

    [CanBeNull]
    private ICSharpParametersOwnerDeclaration ParametersOwnerDeclaration { get; set; }

    [CanBeNull]
    private IFormalParameterList FormalParameterList { get; set; }

    public IEnumerable<IntentionAction> CreateBulbItems()
    {
      if (this.ParametersOwnerDeclaration == null || this.FormalParameterList == null) yield break;

      var anchor = new SubmenuAnchor(HelperActionsConstants.ContextActionsAnchor, SubmenuBehavior.Executable);

      var chopAction = new ChopMethodArgumentsAction(this.ParametersOwnerDeclaration, this.FormalParameterList).ToContextActionIntention(anchor, MyIcons.ContextActionIcon);
      var oneLineAction = new OnelineMethodArgumentsAction(this.ParametersOwnerDeclaration, this.FormalParameterList).ToContextActionIntention(anchor, MyIcons.ContextActionIcon);

      yield return chopAction;
      yield return oneLineAction;
    }

    public bool IsAvailable(IUserDataHolder cache)
    {
      var methodName = this._myProvider.GetSelectedElement<ICSharpIdentifier>();

      var methodDeclaration = MethodDeclarationNavigator.GetByNameIdentifier(methodName);

      ICSharpParametersOwnerDeclaration paramsOwnerDeclaration = methodDeclaration;
      IFormalParameterList paramsList = methodDeclaration?.Params;

      // If unable to resolve by method declaration, try to resolve for ctor.
      if (paramsOwnerDeclaration == null)
      {
        var constructorDeclaration = ConstructorDeclarationNavigator.GetByTypeName(methodName);
        paramsOwnerDeclaration = constructorDeclaration;
        paramsList = constructorDeclaration?.Params;
      }

      if (paramsOwnerDeclaration == null) return false;
      if (paramsOwnerDeclaration.ParameterDeclarations.IsEmpty) return false;

      this.ParametersOwnerDeclaration = paramsOwnerDeclaration;
      this.FormalParameterList = paramsList;

      return true;
    }
  }
}