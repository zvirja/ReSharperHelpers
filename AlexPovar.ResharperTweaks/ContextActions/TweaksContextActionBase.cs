using System.Collections.Generic;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.Util;

namespace AlexPovar.ResharperTweaks.ContextActions
{
  public abstract class TweaksContextActionBase : BulbActionBase, IContextAction
  {
    public IEnumerable<IntentionAction> CreateBulbItems()
    {
      return this.ToContextAction(TweaksActionsPosition.ContextActionsAnchor);
    }

    public abstract bool IsAvailable(IUserDataHolder cache);
  }
}