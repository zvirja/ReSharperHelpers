using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.UI.BulbMenu;

namespace AlexPovar.ResharperTweaks.ContextActions
{
  public static class TweaksActionsPosition
  {
    [NotNull]
    public static IAnchor ContextActionsAnchor { get; } = new InvisibleAnchor(IntentionsAnchors.ContextActionsAnchorPosition.GetNext(), null, true);
  }
}