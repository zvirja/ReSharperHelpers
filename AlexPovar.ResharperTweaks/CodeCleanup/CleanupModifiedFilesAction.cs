using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ActionManagement;
using JetBrains.Application;
using JetBrains.Application.CommandProcessing;
using JetBrains.Application.DataContext;
using JetBrains.Application.Progress;
using JetBrains.DocumentManagers.Transactions;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.Actions;
using JetBrains.ReSharper.Feature.Services.CodeCleanup;
using JetBrains.ReSharper.Features.Altering.CodeCleanup;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.UI.ActionsRevised;
using JetBrains.UI.Application.Progress;
using JetBrains.Util;

namespace AlexPovar.ResharperTweaks.CodeCleanup
{
  [Action("Cleanup modified code...", Icon = typeof(MainThemedIcons.ClearIcon))]
  public class CleanupModifiedFilesAction : CodeCleanupActionBase, IExecutableAction, IInsertLast<IntoSolutionItemGroup_Modify>
  {
    void IExecutableAction.Execute(IDataContext context, DelegateExecute nextExecute)
    {
      var solution = context.GetData(ProjectModelDataConstants.SOLUTION);
      if ((solution != null) && solution.GetPsiServices().Caches.WaitForCaches("CodeCleanupActionBase.Execute"))
      {
        var collector = CodeCleanupFilesCollector.TryCreate(context);
        var actionScope = collector.GetActionScope();
        var profile = SelectProfileDialog(collector, false);
        if (profile != null)
        {
          switch (actionScope)
          {
            case ActionScope.SELECTION:
            case ActionScope.FILE:
            case ActionScope.MULTIPLE_FILES:
            case ActionScope.DIRECTORY:
              return;

            case ActionScope.SOLUTION:
              this.RunFilesFormat(collector, profile);
              return;
          }
        }
      }
    }

    bool IExecutableAction.Update(IDataContext dataContext, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
      var collector = CodeCleanupFilesCollector.TryCreate(dataContext);
      if (collector != null)
      {
        var psiServices = collector.Solution.GetPsiServices();
        if (!psiServices.Files.AllDocumentsAreCommitted || !psiServices.CachesState.IsInitialUpdateFinished.Value)
        {
          return false;
        }

        switch (collector.GetActionScope())
        {
          case ActionScope.NONE:
          case ActionScope.SELECTION:
          case ActionScope.FILE:
          case ActionScope.MULTIPLE_FILES:
          case ActionScope.DIRECTORY:
            return false;

          //Support solution only
          case ActionScope.SOLUTION:
            return true;
        }
      }

      return false;
    }

    private static void FormatFiles(CodeCleanupFilesCollector context, CodeCleanupProfile profile,
      HashSet<FileSystemPath> filesToProcess, CleanupModificationsCounter modificationCounter)
    {
      var solution = context.Solution;
      var files = context.GetFiles();

      var psiFiles = solution.GetPsiServices().Files;
      var codeCleanup = JetBrains.ReSharper.Feature.Services.CodeCleanup.CodeCleanup.GetInstance(solution);
      try
      {
        Shell.Instance.GetComponent<UITaskExecutor>().SingleThreaded.ExecuteTask("Cleanup MODIFIED Code", TaskCancelable.Yes,
          delegate(IProgressIndicator progress)
          {
            using (WriteLockCookie.Create())
            {
              using (Shell.Instance.GetComponent<ICommandProcessor>().UsingBatchTextChange("Code Cleanup"))
              {
                using (var cookie = solution.GetComponent<SolutionDocumentTransactionManager>().CreateTransactionCookie(DefaultAction.Commit, "Code Cleanup"))
                {
                  try
                  {
                    progress.TaskName = profile.Name;
                    progress.Start(files.Count);
                    psiFiles.AssertAllDocumentAreCommitted();
                    foreach (var file in files)
                    {
                      InterruptableActivityCookie.CheckAndThrow(progress);
                      progress.CurrentItemText = file.DisplayName;

                      var fileLocation = file.GetLocation();
                      if (fileLocation.IsEmpty || !filesToProcess.Contains(fileLocation))
                      {
                        progress.Advance(1.0);
                        continue;
                      }

                      var caret = -1;
                      using (var indicator = new SubProgressIndicator(progress, 1.0))
                      {
                        codeCleanup.Run(file, DocumentRange.InvalidRange, ref caret, profile, indicator);
                      }

                      modificationCounter.Increment();
                    }
                  }
                  catch (Exception)
                  {
                    cookie.Rollback();
                    throw;
                  }
                }
              }
            }
          });
      }
      catch (ProcessCancelledException)
      {
      }
    }

    private void RunFilesFormat(CodeCleanupFilesCollector context, CodeCleanupProfile profile)
    {
      HashSet<FileSystemPath> filesToProcess;

      try
      {
        var solutionDir = context.Solution.SolutionFilePath.Directory;
        var gitModificationResolver = new GitModificationsResolver(solutionDir.FullPath);

        if (!gitModificationResolver.IsValidRepository)
        {
          MessageBox.ShowError($"Unable to resolve solution path as a git repository:{Environment.NewLine}{solutionDir.FullPath}");
          return;
        }

        filesToProcess = new HashSet<FileSystemPath>(gitModificationResolver.GetModifiedFiles().Select(FileSystemPath.CreateByCanonicalPath));
      }
      catch (Exception ex)
      {
        MessageBox.ShowError($"Unexpected error in cleanup:{Environment.NewLine}{ex}");
        return;
      }

      var statusBarUpdater = Shell.Instance.GetComponent<StatusBarTextUpdater>();

      if (filesToProcess.Count == 0)
      {
        statusBarUpdater.SetText("Cleanup Modified - No modified files");
        return;
      }

      var modificationCounter = new CleanupModificationsCounter();

      FormatFiles(context, profile, filesToProcess, modificationCounter);

      statusBarUpdater.SetText($"Cleanup Modified - Processed files: {modificationCounter.Count}");
    }

    protected override CodeCleanupProfile GetProfile(CodeCleanupFilesCollector collector)
    {
      throw new NotSupportedException();
    }
  }
}