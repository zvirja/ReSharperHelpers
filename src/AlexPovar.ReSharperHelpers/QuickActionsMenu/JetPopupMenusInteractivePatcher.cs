using JetBrains.Application;
using JetBrains.Application.UI.Controls.JetPopupMenu;
using JetBrains.Lifetimes;
using JetBrains.UI.SrcView.Controls.JetPopupMenu;

namespace AlexPovar.ReSharperHelpers.QuickActionsMenu
{
  [ShellComponent(Lifecycle.DemandReclaimable, Creation.AnyThread, Access.AnyThread)]
  public class JetPopupMenusInteractivePatcher
  {
    public JetPopupMenusInteractivePatcher(JetPopupMenus menus)
    {
      // To not fail in unit tests
      if (menus is JetPopupMenusInteractive menusInteractive)
      {
        menusInteractive.OnViewCreated.Advise(Lifetime.Eternal, (args) =>
        {
          if (args.view is JetPopupMenuOverlordView overlordView)
            ContextMenuSectionNavigationConfigurator.ConfigureMenuView(overlordView.MenuView);
        });
      }
    }
  }
}