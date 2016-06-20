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
      this.ItemCanBeNullAction = new ItemCanBeNullAction(provider);
      this.ItemNotNullAction = new ItemNotNullAction(provider);
    }

    [NotNull]
    private ItemNotNullAction ItemNotNullAction { get; }

    [NotNull]
    private ItemCanBeNullAction ItemCanBeNullAction { get; }

    public IEnumerable<IntentionAction> CreateBulbItems()
    {
      var anchor = new ExecutableGroupAnchor(TweaksActionsConstants.ContextActionsAnchor, null, false);

      if (this.ItemNotNullAction.LastIsAvailableResult)
      {
        yield return this.ItemNotNullAction.ToTweaksAnnotateAction(anchor);
      }

      if (this.ItemCanBeNullAction.LastIsAvailableResult)
      {
        yield return this.ItemCanBeNullAction.ToTweaksAnnotateAction(anchor);
      }
    }


    public bool IsAvailable(IUserDataHolder cache)
    {
      //Both methods should be called, so LastIsAvailableResult is updated for both.
      this.ItemNotNullAction.IsAvailable(cache);
      this.ItemCanBeNullAction.IsAvailable(cache);

      return this.ItemNotNullAction.LastIsAvailableResult || this.ItemCanBeNullAction.LastIsAvailableResult;
    }
  }
}