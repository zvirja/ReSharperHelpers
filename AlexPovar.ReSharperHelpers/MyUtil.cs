using AlexPovar.ReSharperHelpers.Helpers;
using JetBrains.Annotations;
using JetBrains.UI.BulbMenu;

namespace AlexPovar.ReSharperHelpers
{
  public static class MyUtil
  {
    [NotNull]
    public static IAnchor CreateGroupAnchor([NotNull] IAnchor ownerAnchor)
    {
      return new SubmenuAnchor(ownerAnchor, SubmenuBehavior.Executable);
    }

    [NotNull]
    public static IAnchor CreateHelperActionsGroupAnchor() => CreateGroupAnchor(HelperActionsConstants.ContextActionsAnchor);
  }
}