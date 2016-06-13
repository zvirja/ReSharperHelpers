using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.UI.BulbMenu;
using JetBrains.Util;

namespace AlexPovar.ResharperTweaks.ContextActions.ContainerNullability
{
  [ContextAction(Group = "C#", Name = "[Tweaks] Set ItemCanBeNull/ItemNotNull attribute", Description = "Sets ItemCanBeNull/ItemNotNull annotation attribute.")]
  public class ContainerNullabilityGroupAction : IContextAction
  {
    public ContainerNullabilityGroupAction([NotNull] ICSharpContextActionDataProvider provider)
    {
      ItemCanBeNullAction = new ItemCanBeNullAction(provider);
      ItemNotNullAction = new ItemNotNullAction(provider);
    }

    [NotNull]
    private ItemNotNullAction ItemNotNullAction { get; }

    [NotNull]
    private ItemCanBeNullAction ItemCanBeNullAction { get; }

    public IEnumerable<IntentionAction> CreateBulbItems()
    {
      var anchor = new ExecutableGroupAnchor(TweaksActionsConstants.ContextActionsAnchor, null, false);

      if (ItemNotNullAction.LastIsAvailableResult)
      {
        yield return ItemNotNullAction.ToTweaksAnnotateAction(anchor);
      }

      if (ItemCanBeNullAction.LastIsAvailableResult)
      {
        yield return ItemCanBeNullAction.ToTweaksAnnotateAction(anchor);
      }
    }


    public bool IsAvailable(IUserDataHolder cache)
    {
      //Both methods should be called, so LastIsAvailableResult is updated for both.
      ItemNotNullAction.IsAvailable(cache);
      ItemCanBeNullAction.IsAvailable(cache);

      return ItemNotNullAction.LastIsAvailableResult || ItemCanBeNullAction.LastIsAvailableResult;
    }
  }
}