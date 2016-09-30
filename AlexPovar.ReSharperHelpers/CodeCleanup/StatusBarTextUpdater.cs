using System.Runtime.InteropServices;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.VsIntegration.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace AlexPovar.ReSharperHelpers.CodeCleanup
{
  [ShellComponent]
  public class StatusBarTextUpdater
  {
    [CanBeNull] private readonly RawVsServiceProvider _serviceProvider;

    //Optional is required for tests. In runtime dependency is always resolved.
    public StatusBarTextUpdater([CanBeNull, Optional] RawVsServiceProvider serviceProvider)
    {
      this._serviceProvider = serviceProvider;
    }

    public void SetText([NotNull] string text)
    {
      var statusBarService = this._serviceProvider?.Value.TryGetService<SVsStatusbar, IVsStatusbar>();
      statusBarService?.SetText(text);
    }
  }
}