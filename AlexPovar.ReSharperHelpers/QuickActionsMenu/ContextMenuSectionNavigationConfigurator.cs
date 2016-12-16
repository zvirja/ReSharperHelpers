using System;
using System.Windows.Forms;
using JetBrains.Annotations;
using JetBrains.UI.PopupMenu.Impl;

namespace AlexPovar.ReSharperHelpers.QuickActionsMenu
{
  public class ContextMenuSectionNavigationConfigurator
  {
    private const int MiddleJumpThreshold = 6;


    private ContextMenuSectionNavigationConfigurator([NotNull] JetPopupMenuView menuView)
    {
      this.MenuView = menuView;
    }

    [NotNull]
    private JetPopupMenuView MenuView { get; }

    public static void ConfigureMenuView([NotNull] JetPopupMenuView menuView)
    {
      new ContextMenuSectionNavigationConfigurator(menuView).Configure();
    }

    private void Configure()
    {
      this.MenuView.KeyDown += this.MenuViewOnKeyDown;
    }

    private void MenuViewOnKeyDown([NotNull] object sender, [NotNull] KeyEventArgs args)
    {
      if (args.Control)
        switch (args.KeyCode)
        {
          case Keys.Down:
            this.MoveToSection(TryFindNextSection);
            break;

          case Keys.Up:
            this.MoveToSection(TryFindPreviousSection);
            break;
        }
    }

    private void MoveToSection([NotNull] Func<JetPopupMenuDoc, int> indexFinder)
    {
      var doc = this.MenuView.Document;
      var newIndex = indexFinder.Invoke(doc);
      if (newIndex != -1)
      {
        doc.SelectedIndex.Value = newIndex;
      }
    }

    private static int TryFindNextSection([NotNull] JetPopupMenuDoc doc)
    {
      var state = GetCurrentState(doc);

      if (state.CurrentIndex < 0) return -1;

      if (GetSectionLength(state.SectionStartIndex, state.SectionEndIndex) >= MiddleJumpThreshold)
      {
        var middleJumpPos = GetMiddleJumpIndex(state.SectionStartIndex, state.SectionEndIndex);
        if (middleJumpPos > state.CurrentIndex) return middleJumpPos;
      }

      //Don't overflow for single section only. Otherwise, for user it will look as jump above current selection.
      if (state.IsSingleSection) return state.CurrentIndex;

      //Skip delimiter
      var nextSectionStart = state.SectionEndIndex + 2;

      //if we are on bottom - overflow.
      if (nextSectionStart > state.ItemsInMenu - 1)
      {
        nextSectionStart = 0;
      }

      return nextSectionStart;
    }

    private static int TryFindPreviousSection([NotNull] JetPopupMenuDoc doc)
    {
      var state = GetCurrentState(doc);

      if (state.CurrentIndex < 0) return -1;

      int targetSectionStart;
      int targetSectionEnd;
      int targetSectionPos;

      //If we are not at the beginning, we will not leave current section
      //Also, we don't allow overflow behavior for single section.
      if (state.CurrentIndex != state.SectionStartIndex || state.IsSingleSection)
      {
        targetSectionStart = state.SectionStartIndex;
        targetSectionEnd = state.SectionEndIndex;
        targetSectionPos = state.CurrentIndex;
      }
      else
      {
        //Skip separator
        var previousSectionEnd = state.SectionStartIndex - 2;

        //If we are on top - overflow.
        if (previousSectionEnd < 0)
        {
          previousSectionEnd = state.ItemsInMenu - 1;
        }

        var previousSectionStart = FindSectionStartIndex(doc, previousSectionEnd);

        targetSectionStart = previousSectionStart;
        targetSectionEnd = previousSectionEnd;
        targetSectionPos = targetSectionEnd;
      }


      var targetSectionLength = GetSectionLength(targetSectionStart, targetSectionEnd);
      if (targetSectionLength < MiddleJumpThreshold) return targetSectionStart;

      var middleJumpIndex = GetMiddleJumpIndex(targetSectionStart, targetSectionEnd);
      if (middleJumpIndex < targetSectionPos) return middleJumpIndex;

      return targetSectionStart;
    }

    private static int GetMiddleJumpIndex(int sectionStartInd, int sectionEndInd)
    {
      return GetSectionLength(sectionStartInd, sectionEndInd)/2 + sectionStartInd;
    }

    private static int GetSectionLength(int sectionStartInd, int sectionEndInd)
    {
      return sectionEndInd - sectionStartInd + 1;
    }

    private static int FindSectionStartIndex([NotNull] JetPopupMenuDoc doc, int currentIndex)
    {
      var allItems = doc.CurrentItems;

      var ind = currentIndex;
      while (ind >= 0)
      {
        if (ind == 0 || allItems[ind - 1].IsSeparator) break;
        ind--;
      }

      return ind;
    }

    private static int FindSectionEndIndex([NotNull] JetPopupMenuDoc doc, int currentIndex)
    {
      var allItems = doc.CurrentItems;

      var ind = currentIndex;
      while (ind < allItems.Count)
      {
        if (ind == allItems.Count - 1 || allItems[ind + 1].IsSeparator) break;
        ind++;
      }

      return ind;
    }

    private static NavigationState GetCurrentState([NotNull] JetPopupMenuDoc doc)
    {
      var currentIndex = doc.SelectedIndex.Value;

      var allItems = doc.CurrentItems;

      return new NavigationState
      {
        CurrentIndex = currentIndex,
        ItemsInMenu = allItems.Count,
        SectionStartIndex = FindSectionStartIndex(doc, currentIndex),
        SectionEndIndex = FindSectionEndIndex(doc, currentIndex)
      };
    }

    private struct NavigationState
    {
      public int CurrentIndex;

      public int SectionStartIndex;

      public int SectionEndIndex;

      public int ItemsInMenu;

      public bool IsSingleSection => this.SectionStartIndex == 0 && this.SectionEndIndex == this.ItemsInMenu - 1;
    }
  }
}