using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.DataContext;
using JetBrains.Application.Settings;
using JetBrains.Application.Threading;
using JetBrains.Application.UI.Actions;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.Application.UI.ActionSystem.ActionsRevised.Menu;
using JetBrains.Diagnostics;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.Actions;
using JetBrains.ReSharper.Feature.Services.CodeCleanup;
using JetBrains.ReSharper.Features.Altering.CodeCleanup;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;

namespace AlexPovar.ReSharperHelpers.CodeCleanup
{
  [Action("Cleanup modified code...", Icon = typeof(MainThemedIcons.ClearIcon))]
  public class CleanupModifiedFilesAction : CodeCleanupActionBase, IExecutableAction, IInsertLast<IntoSolutionItemGroup_Modify>
  {
    void IExecutableAction.Execute(IDataContext context, DelegateExecute nextExecute)
    {
      var solution = context.GetData(ProjectModelDataConstants.SOLUTION);
      if (solution == null || !solution.GetPsiServices().Caches.WaitForCaches("CodeCleanupActionBase.Execute")) return;

      var collector = CodeCleanupFilesCollector.TryCreate(context).NotNull("collector != null");

      context.GetComponent<IShellLocks>().ExecuteOrQueueReadLock(
        "CodeCleanupActionBase.Execute",
        () =>
        {
          if (!solution.GetPsiServices().Caches.WaitForCaches("CodeCleanupActionBase.Execute")) return;

          var actionScope = collector.GetActionScope();

          var profile = this.GetProfile(collector);
          if (profile == null) return;

          if (this.SaveProfileAsRecentlyUsed)
          {
            solution.GetComponent<CodeCleanupSettingsComponent>().SetRecentlyUsedProfileName(
              solution.GetComponent<ISettingsStore>().BindToContextTransient(ContextRange.Smart(collector.GetContext())),
              profile.Name);
          }

          switch (actionScope)
          {
            case ActionScope.MultipleFiles:
            case ActionScope.Solution:
            case ActionScope.Directory:
              var filteredProvider = this.TryGetFilteredProvider(collector);
              if (filteredProvider != null)
              {
                CodeCleanupRunner.CleanupFiles(filteredProvider, profile);
              }

              return;

            case ActionScope.None:
            case ActionScope.Selection:
            case ActionScope.File:
              return;

            default:
              return;
          }
        });
    }

    bool IExecutableAction.Update(IDataContext dataContext, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
      var collector = CodeCleanupFilesCollector.TryCreate(dataContext);
      if (collector == null) return false;

      var psiServices = collector.Solution.GetPsiServices();
      if (!psiServices.Files.AllDocumentsAreCommitted || !psiServices.CachesState.IsInitialUpdateFinished.Value) return false;

      switch (collector.GetActionScope())
      {
        case ActionScope.None:
        case ActionScope.Selection:
        case ActionScope.File:
          return false;

        case ActionScope.MultipleFiles:
        case ActionScope.Solution:
        case ActionScope.Directory:
          return true;
      }

      return false;
    }

    protected override CodeCleanupProfile GetProfile(CodeCleanupFilesCollector cleanupFilesCollector)
    {
      return cleanupFilesCollector.Solution.GetComponent<CodeCleanupProfileSelector>()
        .SelectProfile(cleanupFilesCollector, false);
    }

    protected override bool SaveProfileAsRecentlyUsed => true;

    [CanBeNull]
    private ICodeCleanupFilesProvider TryGetFilteredProvider([NotNull] ICodeCleanupFilesProvider originalProvider)
    {
      try
      {
        var solutionDir = originalProvider.Solution.SolutionFilePath.Directory.FullPath;
        var gitModificationResolver = new GitModificationsResolver(solutionDir);

        if (!gitModificationResolver.IsValidRepository)
        {
          MessageBox.ShowError($"[ReSharper Helpers] Unable to resolve solution path as a git repository:{Environment.NewLine}{solutionDir}");
          return null;
        }

        var modifiedFiles = gitModificationResolver
          .GetModifiedFiles()
          .Select(f => FileSystemPath.Parse(f, FileSystemPathInternStrategy.TRY_GET_INTERNED_BUT_DO_NOT_INTERN))
          .ToSet();

        var filteredFiles = originalProvider.GetFiles()
          .Where(file =>
          {
            var loc = file.GetLocation();
            return !loc.IsEmpty && modifiedFiles.Contains(loc);
          })
          .ToArray();

        return new FilteredCodeCleanupProvider(originalProvider, filteredFiles);
      }
      catch (Exception ex)
      {
        MessageBox.ShowError($"[ReSharper Helpers] Unexpected error in code cleanup initialization:{Environment.NewLine}{ex}");
        return null;
      }
    }

    private class FilteredCodeCleanupProvider : ICodeCleanupFilesProvider
    {
      [NotNull] private readonly ICodeCleanupFilesProvider _innerProvider;
      [NotNull] private readonly IList<IPsiSourceFile> _filteredFiles;

      public FilteredCodeCleanupProvider([NotNull] ICodeCleanupFilesProvider innerProvider, [NotNull] IList<IPsiSourceFile> filteredFiles)
      {
        this._innerProvider = innerProvider ?? throw new ArgumentNullException(nameof(innerProvider));
        this._filteredFiles = filteredFiles ?? throw new ArgumentNullException(nameof(filteredFiles));
      }

      public IList<IPsiSourceFile> GetFiles() => this._filteredFiles;

      public bool IsSuitableProjectElement(IProjectModelElement element) => this._innerProvider.IsSuitableProjectElement(element);

      public bool IsSuitableFile(IProjectFile file) => this._innerProvider.IsSuitableFile(file);

      public ISolution Solution => this._innerProvider.Solution;

      public IProjectItem ProjectItem => this._innerProvider.ProjectItem;
    }
  }
}