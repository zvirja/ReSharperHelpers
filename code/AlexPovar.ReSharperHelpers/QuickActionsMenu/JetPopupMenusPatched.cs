using System;
using System.Reflection;
using System.Windows.Forms;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.Interop.NativeHook;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.UI.Application;
using JetBrains.UI.PopupMenu;
using JetBrains.UI.PopupMenu.Impl;
using JetBrains.UI.PopupWindowManager;
using JetBrains.UI.Tooltips;
using JetBrains.UI.Wpf;

namespace AlexPovar.ReSharperHelpers.QuickActionsMenu
{
  [ShellComponent]
  public class JetPopupMenusPatched : JetPopupMenus
  {
    [NotNull] private readonly CreateViewInvokerDelegate _createViewInvoker;

    public JetPopupMenusPatched(
      [NotNull] Lifetime lifetime,
      [NotNull] IIsApplicationActiveState isApplicationActiveState,
      [NotNull] IUIApplicationSimple uiapp,
      [NotNull] IAutomationViewsRegistry automationViewsRegistry,
      [CanBeNull] ITooltipManager tooltipman = null,
      [CanBeNull] IWin32Window ownerwin = null,
      [CanBeNull] IWindowsHookManager windowsHookManager = null,
      [CanBeNull] ISettingsStore settstore = null,
      [CanBeNull] PopupWindowManager popupWindowManager = null)
      : base(lifetime, isApplicationActiveState, uiapp, automationViewsRegistry, tooltipman, ownerwin, windowsHookManager, settstore, popupWindowManager)
    {
      var methodInfo = typeof(JetPopupMenus).GetMethod(nameof(this.CreateView), BindingFlags.Instance | BindingFlags.NonPublic);
      this._createViewInvoker = (CreateViewInvokerDelegate)Delegate.CreateDelegate(typeof(CreateViewInvokerDelegate), methodInfo);
    }

    protected override void ShowCore(JetPopupMenu menu, JetPopupMenu.ShowWhen when, bool isModal, LifetimeDefinition lifetimeDefinitionOptional = null,
      IJetPopupMenuOverlordView parentView = null)
    {
      JetPopupMenuStatusAndViewDef jetPopupMenuStatusAndViewDef = menu.InitViewModel(when, lifetimeDefinitionOptional);
      JetPopupMenuStatus status = jetPopupMenuStatusAndViewDef.Status;
      if (status == JetPopupMenuStatus.ShowPopup)
      {
        LifetimeDefinition viewLifetimeDefinition = jetPopupMenuStatusAndViewDef.ViewLifetimeDefinition;
        Lifetime lifeShow = viewLifetimeDefinition.Lifetime;
        lifeShow.AssertIsAlive();

        JetPopupMenuDoc document = menu.Document;
        IJetPopupMenuOverlordView overlordView = this.CreateView(viewLifetimeDefinition, menu, parentView);
        document?.ItemsContainer.IncomingExpand.Advise(lifeShow, delegate(JetPopupMenuItem item) { this.CreateSubmenu(lifeShow, menu, overlordView, item); });
        overlordView.Show(isModal);
        return;
      }

      if (status == JetPopupMenuStatus.BannerNoItems)
      {
        this.ShowNoItemsBanner(menu);
      }
    }

    [NotNull]
    private IJetPopupMenuOverlordView CreateView([NotNull] LifetimeDefinition defShowView, [NotNull] JetPopupMenu menu, [CanBeNull] IJetPopupMenuOverlordView parentView = null)
    {
      var view = this._createViewInvoker.Invoke(this, defShowView, menu, parentView);

      var known = view as JetPopupMenuOverlordView;
      if (known != null)
      {
        ContextMenuSectionNavigationConfigurator.ConfigureMenuView(known.MenuView);
      }

      return view;
    }

    private delegate IJetPopupMenuOverlordView CreateViewInvokerDelegate([NotNull] JetPopupMenus @this, [NotNull] LifetimeDefinition defShowView, [NotNull] JetPopupMenu menu,
      [CanBeNull] IJetPopupMenuOverlordView parentView);
  }
}