using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.UI.BulbMenu;
using JetBrains.UI.Icons;

namespace AlexPovar.ResharperTweaks.ContextActions
{
  public static class TweaksActionsConstants
  {
    [NotNull]
    public static IAnchor ContextActionsAnchor { get; } = new InvisibleAnchor(IntentionsAnchors.ContextActionsAnchorPosition.GetNext(), null, true);

    public static IconId ContextActionIcon => MainThemedIcons.TweaksContextAction.Id;
  }
}