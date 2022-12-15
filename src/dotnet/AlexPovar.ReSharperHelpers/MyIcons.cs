using JetBrains.Annotations;
using JetBrains.UI.Icons;

namespace AlexPovar.ReSharperHelpers
{
  public static class MyIcons
  {
    [NotNull]
    public static IconId ContextActionIcon =>
#if RESHARPER
      AlexPovar.ReSharperHelpers.VisualStudio.MainThemedIcons.HelpersContextAction.Id;
#else
      JetBrains.ReSharper.Feature.Services.Resources.BulbThemedIcons.ContextAction.Id;
#endif
  }
}
