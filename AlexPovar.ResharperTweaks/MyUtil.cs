using AlexPovar.ResharperTweaks.ContextActions;
using JetBrains.Annotations;
using JetBrains.UI.BulbMenu;

namespace AlexPovar.ResharperTweaks
{
  public static class MyUtil
  {
    [NotNull]
    public static IAnchor CreateGroupAnchor([NotNull] IAnchor ownerAnchor)
    {
      return new ExecutableGroupAnchor(ownerAnchor, null, false);
    }

    [NotNull]
    public static IAnchor CreateTweaksGroupAnchor() => CreateGroupAnchor(TweaksActionsConstants.ContextActionsAnchor);
  }
}