using System.Collections.Generic;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.Util;

namespace AlexPovar.ReSharperHelpers.ContextActions
{
  public abstract class HelpersContextActionBase : BulbActionBase, IContextAction
  {
    public IEnumerable<IntentionAction> CreateBulbItems()
    {
      return this.ToContextAction(HelperActionsConstants.ContextActionsAnchor, MyIcons.ContextActionIcon);
    }

    public abstract bool IsAvailable(IUserDataHolder cache);
  }
}