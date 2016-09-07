using System;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.Components;
using JetBrains.Application.Interop.NativeHook;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.UI.Application;
using JetBrains.UI.PopupMenu;
using JetBrains.UI.PopupWindowManager;
using JetBrains.UI.Tooltips;
using JetBrains.UI.Wpf;

namespace AlexPovar.ReSharperHelpers.QuickActionsMenu
{
  [ShellComponent]
  public class JetPopupMenusPatched : JetPopupMenus, IHideImplementation<JetPopupMenus>
  {
    [NotNull] private readonly Func<JetPopupMenu> _factory;

    public JetPopupMenusPatched(
      [NotNull] Lifetime lifetime,
      [NotNull] IIsApplicationActiveState isApplicationActiveState,
      [NotNull] IUIApplicationSimple uiapp,
      [NotNull] IAutomationViewsRegistry automationViewsRegistry,
      [CanBeNull] ITooltipManager tooltipman = null,
      [CanBeNull] IMainWindow mainwin = null,
      [CanBeNull] IWindowsHookManager windowsHookManager = null,
      [CanBeNull] PopupWindowManager popupWindowManager = null,
      [CanBeNull] ISettingsStore settstore = null)
      : base(lifetime, isApplicationActiveState, uiapp, automationViewsRegistry, tooltipman, mainwin, windowsHookManager, popupWindowManager, settstore)
    {
      this._factory = () =>
      {
        lifetime.AssertIsAlive();
        return new JetPopupMenu(
          lifetime,
          uiapp,
          isApplicationActiveState,
          tooltipman,
          mainwin.TryGetActiveWindow(),
          windowsHookManager,
          popupWindowManager,
          settstore,
          automationViewsRegistry,
          new BulbJetPopupMenuComposerPatched());
      };
    }

    public override JetPopupMenu CreateBulbWithGoto()
    {
      return this._factory.Invoke();
    }
  }
}