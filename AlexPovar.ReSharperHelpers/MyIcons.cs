using JetBrains.Annotations;
using JetBrains.UI.Icons;

namespace AlexPovar.ReSharperHelpers
{
  public static class MyIcons
  {
    [NotNull]
    public static IconId ContextActionIcon => MainThemedIcons.HelpersContextAction.Id;

    [NotNull]
    public static IconId YellowBulbIcon => MainThemedIcons.HelpersYellowBulbIcon.Id;

    [NotNull]
    public static IconId EditIcon => MainThemedIcons.HelpersEditIcon.Id;
  }
}