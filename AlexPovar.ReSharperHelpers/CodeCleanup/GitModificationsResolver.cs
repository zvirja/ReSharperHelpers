using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using LibGit2Sharp;

namespace AlexPovar.ReSharperHelpers.CodeCleanup
{
  public class GitModificationsResolver
  {
    public GitModificationsResolver([NotNull] string repoPath)
    {
      this.RepoPath = Repository.Discover(repoPath);

      this.IsValidRepository = Repository.IsValid(this.RepoPath);
    }

    [CanBeNull]
    private string RepoPath { get; }

    public bool IsValidRepository { get; }

    [NotNull]
    public IEnumerable<string> GetModifiedFiles()
    {
      if (!this.IsValidRepository)
      {
        throw new InvalidOperationException("This repository is not valid.");
      }

      var filePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      using (var repo = new Repository(this.RepoPath))
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