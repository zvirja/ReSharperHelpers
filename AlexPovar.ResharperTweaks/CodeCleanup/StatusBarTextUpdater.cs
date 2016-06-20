using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.VsIntegration.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace AlexPovar.ResharperTweaks.CodeCleanup
{
  [ShellComponent]
  public class StatusBarTextUpdater
  {
    private readonly RawVsServiceProvider _serviceProvider;

    public StatusBarTextUpdater([NotNull] RawVsServiceProvider serviceProvider)
    {
      this._serviceProvider = serviceProvider;
    }

    public void SetText([NotNull] string text)
    {
      var statusBarService = this._serviceProvider.Value.TryGetService<SVsStatusbar, IVsStatusbar>();
      statusBarService?.SetText(text);
    }
  }
}