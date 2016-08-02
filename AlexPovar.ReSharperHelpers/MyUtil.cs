using AlexPovar.ReSharperHelpers.ContextActions;
using JetBrains.Annotations;
using JetBrains.UI.BulbMenu;

namespace AlexPovar.ReSharperHelpers
{
  public static class MyUtil
  {
    [NotNull]
    public static IAnchor CreateGroupAnchor([NotNull] IAnchor ownerAnchor)
    {
      return new ExecutableGroupAnchor(ownerAnchor, null, false);
    }

    [NotNull]
    public static IAnchor CreateHelperActionsGroupAnchor() => CreateGroupAnchor(HelperActionsConstants.ContextActionsAnchor);
  }
}