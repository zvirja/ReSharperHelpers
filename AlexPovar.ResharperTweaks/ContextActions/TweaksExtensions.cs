using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.UI.BulbMenu;

namespace AlexPovar.ResharperTweaks.ContextActions
{
  public static class TweaksExtensions
  {
    [NotNull]
    public static IntentionAction ToTweaksAnnotateAction([NotNull] this IBulbAction action, IAnchor anchor = null)
    {
      var text = action.Text;
      anchor = anchor ?? IntentionsAnchors.AnnotateActionsAnchor;
      var iconId = MainThemedIcons.TweaksEditIcon.Id;
      return new IntentionAction(action, text, iconId, anchor);
    }
  }
}