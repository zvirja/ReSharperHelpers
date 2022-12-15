using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
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

    [Pure]
    public static (T value, bool hasAny, bool isSingle) TryGetSingleCandidate<T>(this IEnumerable<T> enumerable)
    {
      using IEnumerator<T> enumerator = enumerable.GetEnumerator();

      T value = default;
      int count = 0;

      while (enumerator.MoveNext())
      {
        count++;

        if (count == 1)
          value = enumerator.Current;
        else if (count > 1)
          break;
      }

      return (value, count > 0, count == 1);
    }
  }
}
