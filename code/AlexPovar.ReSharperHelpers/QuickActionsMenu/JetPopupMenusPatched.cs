using System.Runtime.InteropServices;
using System.Windows.Forms;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.Interop.NativeHook;
using JetBrains.Application.Settings;
using JetBrains.Application.UI.Automation;
using JetBrains.Application.UI.Components;
using JetBrains.Application.UI.Controls.JetPopupMenu;
using JetBrains.Application.UI.Tooltips;
using JetBrains.DataFlow;
using JetBrains.UI.PopupLayout;
using JetBrains.UI.SrcView.Controls.JetPopupMenu;

namespace AlexPovar.ReSharperHelpers.QuickActionsMenu
{
  [ShellComponent(Lifecycle.DemandReclaimable, Creation.AnyThread, Access.AnyThread)]
  public class JetPopupMenusInteractivePatched : JetPopupMenusInteractive
  {
    public JetPopupMenusInteractivePatched(
      [NotNull] Lifetime lifetime,
      [NotNull] IIsApplicationActiveState isApplicationActiveState,
      [NotNull] IUIApplicationSimple uiapp,
      [NotNull] IAutomationViewsRegistry automationViewsRegistry,
      [CanBeNull, Optional] ITooltipManager tooltipman,
      [CanBeNull, Optional] IWin32Window ownerwin,
      [CanBeNull, Optional] IWindowsHookManager windowsHookManager,
      [CanBeNull, Optional] ISettingsStore settstore,
      [CanBeNull, Optional] PopupWindowManager popupWindowManager)
      : base(lifetime, isApplicationActiveState, uiapp, automationViewsRegistry, tooltipman, ownerwin, windowsHookManager, settstore, popupWindowManager)
    {
    }

    [NotNull]
    protected override IJetPopupMenuOverlordView CreateView(LifetimeDefinition defShowView, JetPopupMenu menu, IJetPopupMenuOverlordView parentView = null)
    {
      var result = base.CreateView(defShowView, menu, parentView);
      if (result is JetPopupMenuOverlordView overlordView)
      {
        ContextMenuSectionNavigationConfigurator.ConfigureMenuView(overlordView.MenuView);
      }

      return result;
    }
  }
}