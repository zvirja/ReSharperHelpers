using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.UI.BulbMenu;

namespace AlexPovar.ReSharperHelpers.ContextActions
{
  public static class HelpersExtensions
  {
    [NotNull]
    public static IntentionAction ToHelpersAnnotateAction([NotNull] this IBulbAction action, IAnchor anchor = null)
    {
      var text = action.Text;
      anchor = anchor ?? IntentionsAnchors.AnnotateActionsAnchor;
      var iconId = MyIcons.EditIcon;
      return new IntentionAction(action, text, iconId, anchor);
    }
  }
}