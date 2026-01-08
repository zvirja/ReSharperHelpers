using System.Diagnostics.CodeAnalysis;
using JetBrains.Application.I18n;
using JetBrains.Lifetimes;

namespace AlexPovar.ReSharperHelpers.Resources;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static class PluginStrings
{
  public static JetResourceManager ResourceManager { get; private set; }

  static PluginStrings()
  {
    CultureContextComponent.Instance.Change.Advise(Lifetime.Eternal, args =>
    {
      ResourceManager = args.New.CreateResourceManager("AlexPovar.ReSharperHelpers.Resources.PluginStrings", typeof(PluginStrings).Assembly);
    });
  }

  public static string Cleanup_git_modified_code => ResourceManager.GetString("Cleanup_git_modified_code");
}
