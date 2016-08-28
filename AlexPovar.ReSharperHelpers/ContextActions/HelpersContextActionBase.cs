using System.Collections.Generic;
using AlexPovar.ReSharperHelpers.Helpers;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.Intentions;

namespace AlexPovar.ReSharperHelpers.ContextActions
{
  public abstract class HelpersContextActionBase : ContextActionBase
  {
    public override IEnumerable<IntentionAction> CreateBulbItems()
    {
      return this.ToHelpersContextActionIntentions();
    }
  }
}