using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.UI.BulbMenu;
using JetBrains.UI.Icons;

namespace AlexPovar.ReSharperHelpers.Helpers
{
  public static class HelpersExtensions
  {
    [NotNull, Pure]
    public static IList<IntentionAction> ToHelpersContextActionIntentions([NotNull] this IBulbAction action, [CanBeNull] IAnchor customAnchor = null, [CanBeNull] IconId customIcon = null)
    {
      return new[]
      {
        action.ToHelpersContextActionIntention(customAnchor, customIcon)
      };
    }

    [NotNull, Pure]
    public static IntentionAction ToHelpersContextActionIntention([NotNull] this IBulbAction action, [CanBeNull] IAnchor customAnchor = null, [CanBeNull] IconId customIcon = null)
    {
      return action.ToContextActionIntention(customAnchor ?? HelperActionsConstants.ContextActionsAnchor, customIcon ?? MyIcons.ContextActionIcon);
    }
  }
}