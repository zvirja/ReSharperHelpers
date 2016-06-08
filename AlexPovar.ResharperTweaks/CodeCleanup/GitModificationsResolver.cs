using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using LibGit2Sharp;

namespace AlexPovar.ResharperTweaks.CodeCleanup
{
  public class GitModificationsResolver
  {
    public GitModificationsResolver([NotNull] string repoPath)
    {
      RepoPath = Repository.Discover(repoPath);

      IsValidRepository = Repository.IsValid(RepoPath);
    }

    [CanBeNull]
    private string RepoPath { get; }

    public bool IsValidRepository { get; }

    [NotNull]
    public IEnumerable<string> GetModifiedFiles()
    {
      if (!IsValidRepository)
      {
        throw new InvalidOperationException("This repository is not valid.");
      }

      var filePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      using (var repo = new Repository(RepoPath))
      {
        var status = repo.RetrieveStatus();
        var allNotIgnoredFiles = status.Where(s => s.State != FileStatus.Ignored && s.State != FileStatus.Unaltered);

        foreach (var statusEntry in allNotIgnoredFiles)
        {
          filePaths.Add(statusEntry.FilePath);
        }

        var workDirPath = repo.Info.WorkingDirectory;
        if (workDirPath == null)
        {
          throw new InvalidOperationException("Unable to resolve WorkingDirectory path for repository.");
        }

        return filePaths.Select(relPath => Path.Combine(workDirPath, relPath));
      }
    }
  }
}