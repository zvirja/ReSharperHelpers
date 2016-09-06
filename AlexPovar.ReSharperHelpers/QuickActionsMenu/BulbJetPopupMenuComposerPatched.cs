using System;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using JetBrains.DataFlow;
using JetBrains.UI.Controls.JetPopupMenu.Impl;
using JetBrains.UI.PopupMenu.Impl;

namespace AlexPovar.ReSharperHelpers.QuickActionsMenu
{
  public class BulbJetPopupMenuComposerPatched : BulbJetPopupMenuComposer
  {
    public override void InitUiPart(LifetimeDefinition viewLifetimeDefinition)
    {
      base.InitUiPart(viewLifetimeDefinition);

      this.MenuView.KeyDown += this.MenuViewOnKeyDown;
    }

    private void MenuViewOnKeyDown([NotNull] object sender, [NotNull] KeyEventArgs args)
    {
      if (args.Control)
        switch (args.KeyCode)
        {
          case Keys.Down:
            this.MoveToNextSection(TryFindNextSection);
            break;

          case Keys.Up:
            this.MoveToNextSection(TryFindPreviousSection);
            break;
        }
    }

    private void MoveToNextSection([NotNull] Func<JetPopupMenuDoc, int> indexFinder)
    {
      var newIndex = indexFinder.Invoke(this.MenuDocument);
      if (newIndex != -1)
      {
        this.MenuDocument.SelectedIndex.Value = newIndex;
      }
    }

    private static int TryFindNextSection([NotNull] JetPopupMenuDoc doc)
    {
      var returnNext = false;

      var startIndex = doc.SelectedIndex.Value;

      var currentIndex = startIndex;

      while ((currentIndex = doc.GetNextIndex(currentIndex, JetPopupMenuDoc.NextItemDir.Forward, JetPopupMenuDoc.EndBehavior.WrapAround, JetPopupMenuDoc.AllowedItems.All)) > -1)
      {
        //Return if previous was delimiter.
        if (returnNext) return currentIndex;

        if (currentIndex <= startIndex)
        {
          //Overflow happened. If we have single section only - keep the original position.
          //Otherwise, user will see jump to the beginning.
          if (!doc.CurrentItems.Any(e => e.IsSeparator))
          {
            return -1;
          }

          return currentIndex;
        }

        var element = doc.CurrentItems[currentIndex];
        if (element.IsSeparator)
        {
          returnNext = true;
        }
      }

      return -1;
    }

    private static int TryFindPreviousSection([NotNull] JetPopupMenuDoc doc)
    {
      var startIndex = doc.SelectedIndex.Value;

      var currentIndex = startIndex;

      while ((currentIndex = doc.GetNextIndex(currentIndex, JetPopupMenuDoc.NextItemDir.Backward, JetPopupMenuDoc.EndBehavior.WrapAround, JetPopupMenuDoc.AllowedItems.All)) > -1)
      {
        if ((currentIndex == 0) || doc.CurrentItems[currentIndex - 1].IsSeparator) return currentIndex;
      }

      return -1;
    }
  }
}