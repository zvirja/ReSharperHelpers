using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.UI.BulbMenu;

namespace AlexPovar.ReSharperHelpers.Helpers
{
  public static class HelperActionsConstants
  {
    [NotNull]
    public static IAnchor ContextActionsAnchor { get; } = IntentionsAnchors.ContextActionsAnchor.CreateNext(true);
  }
}