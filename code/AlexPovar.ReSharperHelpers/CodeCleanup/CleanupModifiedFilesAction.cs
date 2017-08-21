using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AlexPovar.ReSharperHelpers.Helpers;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.CommandProcessing;
using JetBrains.Application.DataContext;
using JetBrains.Application.Progress;
using JetBrains.Application.UI.Actions;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.Application.UI.ActionSystem.ActionsRevised.Menu;
using JetBrains.Application.UI.Progress;
using JetBrains.DocumentManagers.Transactions;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.Actions;
using JetBrains.ReSharper.Feature.Services.CodeCleanup;
using JetBrains.ReSharper.Features.Altering.CodeCleanup;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Util;

namespace AlexPovar.ReSharperHelpers.CodeCleanup
{
  [Action("Cleanup modified code...", Icon = typeof(MainThemedIcons.ClearIcon))]
  public class CleanupModifiedFilesAction : CodeCleanupActionBase, IExecutableAction, IInsertLast<IntoSolutionItemGroup_Modify>
  {
    void IExecutableAction.Execute(IDataContext context, DelegateExecute nextExecute)
    {
      var solution = context.GetData(ProjectModelDataConstants.SOLUTION);
      if (solution != null && solution.GetPsiServices().Caches.WaitForCaches("CodeCleanupActionBase.Execute"))
      {
        var collector = CodeCleanupFilesCollector.TryCreate(context).NotNull("collector != null");

        var actionScope = collector.GetActionScope();
        var profile = this.GetProfile(collector);
        if (profile != null)
        {
          switch (actionScope)
          {
            case ActionScope.Selection:
            case ActionScope.File:
              return;

            case ActionScope.MultipleFiles:
            case ActionScope.Solution:
            case ActionScope.Directory:
              this.RunFilesFormat(collector, profile);
              return;
          }
        }
      }
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

    public static MethodInfo GetSelectProfileWithWpfDialogMethod()
    {
      return typeof(CodeCleanupRunner)
        .Assembly.GetType("JetBrains.ReSharper.Features.Altering.CodeCleanup.InteractiveProfileSelector")
        .GetMethod("SelectProfileWithWpfDialog", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
    }

    protected override CodeCleanupProfile GetProfile(CodeCleanupFilesCollector cleanupFilesCollector)
    {
      return (CodeCleanupProfile)GetSelectProfileWithWpfDialogMethod()
        .Invoke(null, new object[]
        {
          cleanupFilesCollector,
          false,
        });
    }

    protected override bool SaveProfileAsRecentlyUsed => true;

    [CopyFromOriginal("JetBrains.ReSharper.Features.Altering.CodeCleanup.CodeCleanupRunner.CleanupFilesWithProgress()")]
    private static void FormatFiles([NotNull] CodeCleanupFilesCollector context, [NotNull] CodeCleanupProfile profile,
      /* START_MOD */ [NotNull] ISet<FileSystemPath> filesToProcess /* END_MOD */)
    {
      try
      {
        Shell.Instance.GetComponent<UITaskExecutor>()
          .SingleThreaded.ExecuteTask( /*START_MOD*/ "Cleanup MODIFIED Code" /*END_MOD*/, TaskCancelable.Yes, delegate(IProgressIndicator progress)
          {
            ISolution solution = context.Solution;
            IList<IPsiSourceFile> files = context.GetFiles();
            IPsiFiles psiFiles = solution.GetPsiServices().Files;
            JetBrains.ReSharper.Feature.Services.CodeCleanup.CodeCleanup codeCleanup = JetBrains.ReSharper.Feature.Services.CodeCleanup.CodeCleanup.GetInstance(solution);

            using (Shell.Instance.GetComponent<ICommandProcessor>().UsingBatchTextChange("Code Cleanup"))
            {
              using (ITransactionCookie transactionCookie = solution.GetComponent<SolutionDocumentTransactionManager>().CreateTransactionCookie(DefaultAction.Commit, "Code Cleanup"))
              {
                try
                {
                  progress.TaskName = profile.Name;
                  progress.Start(files.Count);
                  psiFiles.AssertAllDocumentAreCommitted();
                  foreach (IPsiSourceFile file in files)
                  {
                    InterruptableActivityCookie.CheckAndThrow(progress);

                    /* START_MOD */
                    var fileLocation = file.GetLocation();
                    if (fileLocation.IsEmpty || !filesToProcess.Contains(fileLocation))
                    {
                      progress.Advance(1.0);
                      continue;
                    }
                    /* END_MOD */

                    progress.CurrentItemText = file.DisplayName;
                    using (SubProgressIndicator subProgressIndicator = new SubProgressIndicator(progress, 1.0))
                    {
                      int caret = -1;
                      codeCleanup.Run(file, DocumentRange.InvalidRange, ref caret, profile, subProgressIndicator);
                    }
                  }
                  progress.Stop();
                }
                catch (Exception)
                {
                  transactionCookie.Rollback();
                  throw;
                }
              }
            }
          });
      }
      catch (ProcessCancelledException)
      {
      }
    }

    private void RunFilesFormat([NotNull] CodeCleanupFilesCollector context, [NotNull] CodeCleanupProfile profile)
    {
      ISet<FileSystemPath> filesToProcess;

      try
      {
        var solutionDir = context.Solution.SolutionFilePath.Directory.FullPath;
        var gitModificationResolver = new GitModificationsResolver(solutionDir);

        if (!gitModificationResolver.IsValidRepository)
        {
          MessageBox.ShowError($"Unable to resolve solution path as a git repository:{Environment.NewLine}{solutionDir}");
          return;
        }

        filesToProcess = gitModificationResolver
          .GetModifiedFiles()
          .Select(f => FileSystemPath.Parse(f, FileSystemPathInternStrategy.TRY_GET_INTERNED_BUT_DO_NOT_INTERN))
          .ToSet();
      }
      catch (Exception ex)
      {
        MessageBox.ShowError($"Unexpected error in cleanup:{Environment.NewLine}{ex}");
        return;
      }

      if (filesToProcess.Count == 0)
      {
        return;
      }

      FormatFiles(context, profile, filesToProcess);
    }
  }
}