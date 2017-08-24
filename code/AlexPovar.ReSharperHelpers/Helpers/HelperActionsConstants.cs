using JetBrains.Annotations;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.ReSharper.Feature.Services.Intentions;

namespace AlexPovar.ReSharperHelpers.Helpers
{
  public static class HelperActionsConstants
  {
    [NotNull]
    public static IAnchor ContextActionsAnchor { get; } = IntentionsAnchors.ContextActionsAnchor.CreateNext(true);
  }
}